package ae.dubaiinvestments.vms.ui

/**
 * What a person typed, and whether it can be believed - the tablet's copy.
 *
 * The rules themselves belong to the server, in `Services/VisitorFields.cs`, and that is
 * where a save is actually judged. This is here because a tablet cannot call C# and because
 * being told after a round trip is being told too late: the officer has moved on, the
 * visitor is waiting, and the message arrives attached to a save rather than to a field.
 *
 * So the two exist for different jobs and are allowed to differ in exactly one direction.
 * This one may be quieter than the server - it never accepts something the server would
 * refuse, because the server has the final say either way - but it must never refuse
 * something the server would take, which would strand the desk with no way forward.
 *
 * The same split as the server:
 *
 * **A problem blocks.** Only the Emirates ID number, because it is what a repeat visit is
 * matched on and it carries its own check digit. A wrong one does not make a bad record, it
 * makes a second person.
 *
 * **A concern does not.** A mobile number in an odd shape, a date in a format the report
 * cannot read, a card that has expired: all worth saying out loud at the desk and none of
 * them worth refusing a visitor over.
 */
object FieldRules {

    /** The column widths, from `Data/FieldLengths.cs`. Typing is capped at them so a long
        paste is refused here rather than silently cut to fit when the row is written. */
    const val IdNumber = 30
    const val CardNumber = 30
    const val Name = 300
    const val PersonToVisit = 200
    const val PurposeOther = 200
    const val CardField = 150
    const val ContactMobile = 40

    const val EmiratesIdDigits = 15
    const val EmiratesIdPrefix = "784"

    fun digits(raw: String?): String = raw?.filter(Char::isDigit) ?: ""

    /**
     * Why a typed Emirates ID number cannot be accepted, or null. Blocking.
     *
     * Typed only. A chip read carries a signed document and a photographed card has already
     * satisfied this same check digit on the server; the card is the authority on what is
     * printed on the card, and neither is asked to prove itself twice.
     */
    fun typedIdNumberProblem(typed: String?): String? {
        val digits = digits(typed)

        return when {
            digits.isEmpty() -> "An ID number is required."

            digits.length != EmiratesIdDigits ->
                "An Emirates ID number is $EmiratesIdDigits digits. That is ${digits.length}."

            !digits.startsWith(EmiratesIdPrefix) ->
                "An Emirates ID number begins $EmiratesIdPrefix."

            /* The fifteenth digit is a Luhn checksum over the first fourteen, so a single
               wrong digit and almost every swap of two adjacent ones is caught at the desk
               rather than in a report months later. */
            !luhn(digits) ->
                "That is fifteen digits but not a valid Emirates ID number - check it " +
                    "against the card."

            else -> null
        }
    }

    /**
     * What is odd about a telephone number, or null. Never blocking.
     *
     * Deliberately generous: visitors give landlines, foreign numbers and numbers with
     * extensions, and a reception desk is the wrong place to argue about any of them. What
     * it catches is the entry that is not a number at all, and the UAE mobile that is a
     * digit short - the mistake that actually happens, and that nothing would otherwise
     * notice until somebody tried to ring it.
     */
    fun mobileConcern(typed: String?): String? {
        /* Blank is not this rule's business. Whether the field is required is asked in one
           place, and answering it here as well would say it twice. */
        if (typed.isNullOrBlank()) return null

        if (typed.any(Char::isLetter)) {
            return "That has letters in it. A telephone number is digits, with + and spaces " +
                "if they help."
        }

        val digits = digits(typed)

        if (digits.length < 7) return "That looks too short for a telephone number."

        /* E.164 caps a subscriber number at fifteen digits, country code included. */
        if (digits.length > 15) return "That is longer than any telephone number."

        var national = digits
        if (national.startsWith("00")) national = national.drop(2)
        if (national.startsWith("971")) national = national.drop(3)
        national = national.trimStart('0')

        /* UAE mobiles are the 5 series and are nine digits nationally. Landlines and foreign
           numbers fall out of this test rather than into it, which is intended. */
        if (national.startsWith("5") && national.length != 9) {
            return "A UAE mobile is nine digits after the country code - 05x xxx xxxx."
        }

        return null
    }

    /**
     * What is odd about a card expiry date, or null. Never blocking.
     *
     * Two things are worth saying: that the date will not be readable as a date, and that the
     * card has already expired. The second is the one reception would want to know and the
     * one nothing anywhere said before.
     */
    fun expiryConcern(value: String?, today: SimpleDate = todayAtTheDesk()): String? {
        if (value.isNullOrBlank()) return null

        val date = readCardDate(value)
            ?: return "That is not a date the report will be able to read. " +
                "The card prints it as 25/08/2028."

        return if (date < today) "That card expired on ${inWords(date)}." else null
    }

    /** A date, compared by value. java.time would do, but this is three integers and a
        comparison, and it keeps the rule readable beside the server's copy. */
    data class SimpleDate(val year: Int, val month: Int, val day: Int) : Comparable<SimpleDate> {
        override fun compareTo(other: SimpleDate): Int =
            compareValuesBy(this, other, { it.year }, { it.month }, { it.day })
    }

    /** "25 Aug 2028" - how the rest of the screens write a date. */
    fun inWords(date: SimpleDate): String =
        "%02d %s %d".format(date.day, MonthNames[date.month - 1], date.year)

    /**
     * A card date as a date, or null when it is in none of the shapes seen.
     *
     * The zone produces dd/MM/yyyy. What the chip's XML produces is not documented in the
     * toolkit reference and this project has no sample to read, which is exactly why an
     * unrecognised format is a concern and never a refusal.
     */
    fun readCardDate(value: String?): SimpleDate? {
        if (value.isNullOrBlank()) return null
        val text = value.trim()

        Numeric.matchEntire(text)?.let { match ->
            val (a, b, c) = match.destructured
            /* Four digits first means it led with the year; otherwise the day did. Both are
               unambiguous, unlike anything that would let a month lead. */
            return if (a.length == 4) {
                dateOrNull(a.toInt(), b.toInt(), c.toInt())
            } else {
                dateOrNull(c.toInt(), b.toInt(), a.toInt())
            }
        }

        Compact.matchEntire(text)?.let { match ->
            val d = match.value
            return dateOrNull(d.substring(0, 4).toInt(), d.substring(4, 6).toInt(), d.substring(6).toInt())
        }

        Spelled.matchEntire(text)?.let { match ->
            val (day, name, year) = match.destructured
            val month = MonthNames.indexOfFirst { it.equals(name, ignoreCase = true) }
            return if (month < 0) null else dateOrNull(year.toInt(), month + 1, day.toInt())
        }

        return null
    }

    private fun dateOrNull(year: Int, month: Int, day: Int): SimpleDate? {
        if (month !in 1..12 || day < 1) return null
        return if (day > daysIn(month, year)) null else SimpleDate(year, month, day)
    }

    private fun daysIn(month: Int, year: Int): Int = when (month) {
        1, 3, 5, 7, 8, 10, 12 -> 31
        4, 6, 9, 11 -> 30
        else -> if (year % 4 == 0 && (year % 100 != 0 || year % 400 == 0)) 29 else 28
    }

    /**
     * Today at the desk, which is in Dubai.
     *
     * UTC+4 with no daylight saving, so the offset is the whole of it. Four hours matters: a
     * card expiring today reads as expired for the first four hours of the working day if
     * this is UTC.
     */
    fun todayAtTheDesk(): SimpleDate {
        val millis = System.currentTimeMillis() + 4 * 60 * 60 * 1000L
        var days = Math.floorDiv(millis, 86_400_000L)

        /* Days since 1970 to a calendar date, by walking years then months. A few dozen
           iterations once per recomposition, and no dependency on a time zone database the
           tablet may or may not have kept up to date. */
        var year = 1970
        while (true) {
            val inYear = if (year % 4 == 0 && (year % 100 != 0 || year % 400 == 0)) 366 else 365
            if (days < inYear) break
            days -= inYear
            year++
        }

        var month = 1
        while (days >= daysIn(month, year)) {
            days -= daysIn(month, year)
            month++
        }

        return SimpleDate(year, month, (days + 1).toInt())
    }

    /**
     * The first valid Emirates ID number in a block of recognised text, or null.
     *
     * The same search the server does, on the tablet, so the officer sees the number the
     * instant the camera reads it rather than after a round trip. It can be done here
     * precisely because the number carries its own Luhn check digit: there is no judgement in
     * it to get wrong, only arithmetic, and arithmetic gives the same answer on both sides.
     *
     * Found loosely and judged strictly. The front prints it as 784-1980-5919869-1 and a
     * recogniser returns those hyphens, or spaces, or neither, or a line break in the middle
     * of them - the UAE Pass card wraps the number across two lines - so anything that is not
     * a digit between the digits is ignored, and the fifteen that fall out either satisfy
     * Luhn or are not an Emirates ID number.
     */
    fun findIdNumber(text: String?): String? {
        if (text.isNullOrBlank()) return null

        for (match in Candidate.findAll(text.uppercase())) {
            val digits = match.value.filter(Char::isDigit)
            if (digits.length < EmiratesIdDigits) continue

            /* The pattern can over-reach into a number that follows, so the first fifteen
               digits are taken rather than all of them. */
            val candidate = digits.take(EmiratesIdDigits)
            if (typedIdNumberProblem(candidate) == null) return candidate
        }

        return null
    }

    /**
     * Whether this looks like the three lines off the back of a card.
     *
     * Used to decide which frames are worth a round trip. A zone line is thirty characters
     * from a 37-character alphabet, so three lines of twenty or more of those characters is a
     * zone and almost nothing else is - a shirt, a desk or the front of a card does not
     * produce them.
     */
    fun looksLikeZone(text: String?): Boolean =
        (text ?: "").lineSequence()
            .count { line -> line.count { it in ZoneAlphabet } >= 20 } >= 3

    private const val ZoneAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789<"

    /** 784 and fifteen digits in all, with whatever the recogniser put between them. */
    private val Candidate = Regex("""784\D{0,3}(?:\d\D{0,3}){12}""")

    // ------------------------------------------------------------------ typing help

    /*  The separators are put in by the screen, not by the officer.
     *
     *  An Emirates ID number is printed 784-1980-5919869-1 and a card date 25/08/2028, and a
     *  desk copying one off a card should be typing the digits and nothing else. Two hyphens
     *  and a slash each are three more chances to fumble on a soft keyboard, three more
     *  reasons for a number to come out a character wrong, and the field accepts the digits
     *  alone anyway - so asking for them was never anything but friction.
     */

    /** 784-1980-5919869-1, from however many digits there are so far. */
    fun groupIdNumber(digits: String?): String = group(digits, '-', 15, 3, 7, 14)

    /** 25/08/2028, from however many digits there are so far. */
    fun groupCardDate(digits: String?): String = group(digits, '/', 8, 2, 4)

    private fun group(raw: String?, separator: Char, maximum: Int, vararg before: Int): String {
        val digits = digits(raw).take(maximum)
        val built = StringBuilder(digits.length + before.size)

        for (i in digits.indices) {
            if (i in before) built.append(separator)
            built.append(digits[i])
        }

        return built.toString()
    }

    /**
     * What the field should now hold, given what is in it and what was in it before.
     *
     * The one subtlety in a field that formats itself. Backspace over a separator deletes the
     * separator, the digits are unchanged, and re-grouping them puts it straight back - so the
     * field cannot be shortened past it and the officer is stuck pressing a key that does
     * nothing. When the text got shorter and the digits did not, a separator is what was
     * deleted, and the digit in front of it is what was meant.
     */
    fun retype(typed: String, previous: String, group: (String) -> String): String {
        var digits = digits(typed)

        if (typed.length < previous.length && digits == digits(previous)) {
            digits = digits.dropLast(1)
        }

        return group(digits)
    }

    /**
     * Luhn, right to left, doubling every second digit and casting out nines.
     *
     * The same checksum a payment card uses, and the same one `EmiratesIdNumber.cs` applies
     * on the server. It catches every single-digit error and almost every transposition of
     * two adjacent digits, which between them are what a typing finger does to a row of
     * figures.
     */
    private fun luhn(number: String): Boolean {
        var total = 0
        var doubling = false

        for (i in number.indices.reversed()) {
            var digit = number[i] - '0'
            if (doubling) {
                digit *= 2
                if (digit > 9) digit -= 9
            }
            total += digit
            doubling = !doubling
        }

        return total % 10 == 0
    }

    private val MonthNames = listOf(
        "Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
    )

    /** dd/MM/yyyy, dd-MM-yyyy, dd.MM.yyyy and yyyy-MM-dd, which is what the zone and the
        report have been seen to produce. */
    private val Numeric = Regex("""(\d{1,4})[/\-.](\d{1,2})[/\-.](\d{1,4})""")
    private val Compact = Regex("""\d{8}""")
    private val Spelled = Regex("""(\d{1,2})\s+([A-Za-z]{3,})\s+(\d{4})""")
}
