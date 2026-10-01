package ae.dubaiinvestments.vms.ui.parts

import androidx.compose.ui.text.AnnotatedString
import androidx.compose.ui.text.input.OffsetMapping
import androidx.compose.ui.text.input.TransformedText
import androidx.compose.ui.text.input.VisualTransformation

/**
 * Draws the separators without putting them in the field.
 *
 * The first attempt at this reformatted the value itself: type 7841 and the field was set to
 * "784-1". It read correctly and typed appallingly. A Compose text field maps the caret from
 * the old string to the new one, and a string that grew in the middle leaves the caret before
 * the character the officer had just typed - so the next digit landed in front of it and
 * 7841980 came out as 784-91. The desk found that in about ten seconds.
 *
 * There is no arrangement of that approach that works, because the caret is not the value's
 * to decide. So the field holds digits and nothing else - the officer types fifteen digits and
 * fifteen digits is exactly what is in there - and the hyphens are painted over the top by this
 * transformation, with an [OffsetMapping] that tells Compose where each digit ended up.
 *
 * Everything else falls out of that. Backspace deletes a digit because there is nothing else to
 * delete; the old code needed a rule to stop the formatter putting back a separator the officer
 * had just removed, and that rule is gone. Tapping into the middle works. Select-all works.
 *
 * @param separator the character to paint between the groups
 * @param before the digit positions a separator is painted in front of - 3, 7 and 14 for an
 *   Emirates ID number, which is where 784-1980-5919869-1 breaks
 */
class DigitGrouping(
    private val separator: Char,
    private vararg val before: Int,
) : VisualTransformation {

    override fun filter(text: AnnotatedString): TransformedText {
        val digits = text.text

        val shown = StringBuilder(digits.length + before.size)

        /*  Where each digit ends up, worked out while the string is built rather than
         *  calculated afterwards.
         *
         *  One entry per digit plus one for the end, which is the position the caret takes
         *  after the last digit - and the common case, since that is where typing happens. */
        val placed = IntArray(digits.length + 1)

        for (i in digits.indices) {
            if (i in before) shown.append(separator)
            placed[i] = shown.length
            shown.append(digits[i])
        }

        placed[digits.length] = shown.length

        val mapping = object : OffsetMapping {
            /* The caret sits before digit `offset`, which is wherever that digit was placed.
               A separator in front of it is therefore behind the caret, which is what the eye
               expects: "784-|1" after four digits, never "784|-1". */
            override fun originalToTransformed(offset: Int): Int =
                placed[offset.coerceIn(0, digits.length)]

            /* And back: how many digits lie entirely before this point in what is drawn. */
            override fun transformedToOriginal(offset: Int): Int =
                digits.indices.count { placed[it] < offset }
        }

        return TransformedText(AnnotatedString(shown.toString()), mapping)
    }

    companion object {
        /** 784-1980-5919869-1 */
        val EmiratesIdNumber = DigitGrouping('-', 3, 7, 14)

        /** 25/08/2028 */
        val CardDate = DigitGrouping('/', 2, 4)
    }
}
