package ae.dubaiinvestments.vms.card

/**
 * A card, as the chip gave it up.
 *
 * [responseXml] is the point of this type. It is the signed Validation Gateway response,
 * and it is what gets posted to the server; the rest of the fields exist only so the desk
 * can see who is standing there before committing. The server does not believe any of
 * them - it parses its own copy out of the XML - which is exactly the arrangement the web
 * app uses for a read coming from a browser, and for the same reason: this device is not
 * where trust should live.
 *
 * Every field `CardResponseParser` takes out of that XML is taken out here too, deliberately.
 * The stored record was never in question - the server parses the same document whichever
 * client sent it, so a visit recorded from a tablet already carried the home address and the
 * place of birth - but what the officer could *see* stopped at six fields, and the mobile
 * number going missing was the first anyone noticed of it. One list, both clients.
 */
data class CardRead(
    val responseXml: String,

    // -- identity, from the header and the non-modifiable data
    val idNumber: String,
    val cardNumber: String?,
    val idType: String?,
    val fullNameEnglish: String,
    /** Before the comma-joining [fullNameEnglish] does to it, which is how the card stores a
        name and not how anybody writes one. The server keeps both for the same reason. */
    val fullNameRaw: String?,
    val fullNameArabic: String?,
    val titleEnglish: String?,
    val gender: String?,
    val dateOfBirth: String?,
    val placeOfBirthEnglish: String?,
    val nationalityEnglish: String?,
    val nationalityArabic: String?,
    val nationalityCode: String?,
    val issueDate: String?,
    val expiryDate: String?,

    // -- the home address, which the read already asks the chip for
    val addressEmirate: String?,
    val addressCity: String?,
    val addressArea: String?,
    val addressStreet: String?,
    val addressBuilding: String?,
    val addressPoBox: String?,
    val addressPhone: String?,
    /** What the contact box is filled from. The officer overwrites it whenever the visitor
        gives a different number. */
    val addressMobile: String?,
    val addressEmail: String?,

    val photo: ByteArray?,
) {
    /** Whether the chip gave up an address at all. Not every card carries one, and an empty
        panel of nine dashes is worse than no panel. */
    val hasHomeAddress: Boolean
        get() = listOf(
            addressEmirate, addressCity, addressArea, addressStreet, addressBuilding,
            addressPoBox, addressPhone, addressMobile, addressEmail,
        ).any { !it.isNullOrBlank() }

    /* A data class with a ByteArray gets equals() and hashCode() that compare the array
       by identity, which is a trap for anyone who later puts one of these in a set. */
    override fun equals(other: Any?) = this === other
    override fun hashCode() = System.identityHashCode(this)
}

/** What the desk can see about the reader before a card goes in. */
data class ReaderState(
    val readerName: String? = null,
    val cardPresent: Boolean = false,
    val detail: String,
    val toolkitVersion: String? = null,
    val licenceExpiry: String? = null,
)

/**
 * Reading an Emirates ID.
 *
 * An interface because the reader is the one part of this app that is not settled. It is
 * an ACS reader over USB today, which matches the hardware the desks already have and the
 * "insert the card" habit reception already has. If NFC is ever wanted it goes behind
 * this same interface - and it is a different flow, not a different driver: the chip will
 * not release anything over NFC until it is given the card number, date of birth and
 * expiry date, so those have to be read off the printed card first.
 */
interface EmiratesIdReader {
    suspend fun state(): ReaderState

    /**
     * @param requestId issued by the server, spent once. The gateway stamps it into the
     *   signed response, which is what stops a captured response being replayed into a
     *   later check-in.
     */
    suspend fun read(requestId: String, onPhase: (String) -> Unit): CardRead
}

/** Something the desk can act on. Anything else is a bug and should crash. */
class CardReadException(message: String, cause: Throwable? = null) : Exception(message, cause)
