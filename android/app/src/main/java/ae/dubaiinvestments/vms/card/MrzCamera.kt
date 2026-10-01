package ae.dubaiinvestments.vms.card

import ae.dubaiinvestments.vms.ui.FieldRules
import android.annotation.SuppressLint
import android.util.Log
import androidx.camera.core.ImageAnalysis
import androidx.camera.core.ImageProxy
import com.google.mlkit.vision.common.InputImage
import com.google.mlkit.vision.text.TextRecognition
import com.google.mlkit.vision.text.latin.TextRecognizerOptions
import java.util.concurrent.atomic.AtomicBoolean

/**
 * Reads whatever text is in front of the camera and hands it up, one frame at a time.
 *
 * It does not decide anything. What makes a run of characters an Emirates ID - the TD1 check
 * digits, the Luhn digit on the printed number, the positional repair of OCR-B confusions -
 * is arithmetic on the server, where the browser scanner's copy of it already lives. Two
 * implementations of the rule that decides whether a visitor record is real is one too many,
 * and the one that would drift is the one on a device nobody rebuilds.
 *
 * So this is deliberately thin: frames in, text out, and the answer comes back from
 * `/api/mrz`.
 *
 * ML Kit rather than Tesseract. It is on-device and free per read, which is the constraint
 * that ruled out every cloud service, and unlike the browser it needs no help finding the
 * text: it returns blocks with their own bounding boxes, so none of the band-cropping,
 * thresholding and deskewing the web scanner needs exists here.
 */
class MrzAnalyzer(
    private val onText: (String) -> Unit,
    private val onNumberSeen: (String) -> Unit,
) : ImageAnalysis.Analyzer {

    private val recogniser = TextRecognition.getClient(TextRecognizerOptions.DEFAULT_OPTIONS)

    /**
     * One frame at a time.
     *
     * The camera produces frames faster than they can be read, and `STRATEGY_KEEP_ONLY_LATEST`
     * only stops them queuing - it does not stop a second recognition starting while the first
     * is still running. Without this the tablet spends its battery on frames whose answers
     * arrive too late to matter.
     */
    private val busy = AtomicBoolean(false)

    @Volatile
    var running: Boolean = true

    @Volatile
    private var lastSent: String? = null

    /** The last number reported upward, so a card held steady reports it once. */
    @Volatile
    private var lastNumber: String? = null

    /** When a frame carrying only print was last posted. */
    @Volatile
    private var lastPrintSentAt = 0L

    @SuppressLint("UnsafeOptInUsageError")
    override fun analyze(image: ImageProxy) {
        val media = image.image
        if (media == null || !running || !busy.compareAndSet(false, true)) {
            image.close()
            return
        }

        val input = InputImage.fromMediaImage(media, image.imageInfo.rotationDegrees)

        recogniser.process(input)
            .addOnSuccessListener { result ->
                /*  Sorted down the frame, and this is the fix for "it only ever reads the
                 *  number".
                 *
                 *  Line by line rather than result.text, because the zone is three lines and
                 *  the server looks for three consecutive ones. But ML Kit returns blocks in
                 *  its own order, not reading order, and the back of an Emirates ID is not
                 *  only the zone: Card Number, Occupation, Employer and Issuing Place are up
                 *  there too, in English and in Arabic. Flattening blocks in the order given
                 *  interleaved those between the three zone lines, so no run of three was ever
                 *  the zone, every parse failed, and the one thing that survived was the
                 *  printed number - which is exactly what the desk reported.
                 *
                 *  Sorting by where the line actually sits puts the zone back together
                 *  whatever order the blocks arrived in. Top, then left, so a line that wraps
                 *  still reads left to right. */
                val lines = result.textBlocks
                    .flatMap { it.lines }
                    .filter { it.text.isNotBlank() }
                    .sortedWith(
                        compareBy(
                            { it.boundingBox?.top ?: 0 },
                            { it.boundingBox?.left ?: 0 },
                        ),
                    )
                    .map { it.text.trim() }

                if (lines.isEmpty()) return@addOnSuccessListener

                val text = lines.joinToString("\n")

                /*  The number, found here rather than asked about.
                 *
                 *  It carries a Luhn check digit, so there is no judgement in it to get wrong
                 *  - only arithmetic, which gives the same answer on this side as on the
                 *  server's. Reporting it straight away is the difference between a number
                 *  appearing as the card comes into frame and a number appearing after a round
                 *  trip over office wifi, which is the ten seconds the desk was counting.
                 *
                 *  The server is still what the record is built from. This is what the officer
                 *  sees while it is being asked. */
                FieldRules.findIdNumber(text)?.let { number ->
                    if (number != lastNumber) {
                        lastNumber = number
                        onNumberSeen(number)
                    }
                }

                /*  Filtered here, before the network.
                 *
                 *  Every rule about whether text is a card lives on the server, but a round
                 *  trip per frame would make the loop as slow as the connection. These two
                 *  tests are ones no recogniser can be wrong about - the zone is padded with
                 *  chevrons, and the printed number starts 784 - so text with neither is a
                 *  desk, a shirt or a wall, and is dropped without a call. Nothing is sent
                 *  until a card is actually in front of the camera.
                 *
                 *  Deliberately generous: it decides what is worth asking about, never what
                 *  is true. A frame it lets through and the server rejects costs one small
                 *  request; a frame it wrongly drops costs a read. */
                if (!worthSending(text)) return@addOnSuccessListener

                /* The same text arrives repeatedly while a card is held steady. Asking again
                   about an answer already given is the one call guaranteed to be wasted. */
                if (text == lastSent) return@addOnSuccessListener

                /*  A zone goes up at once; print waits its turn.
                 *
                 *  Only the back of a card can fill the whole form, so a frame that looks like
                 *  a zone is worth a round trip every time. A frame that merely shows a number
                 *  is the front, or a phone, and the answer to it will not change while the
                 *  officer holds it there - but the recogniser varies by a character between
                 *  frames, so the dedupe above does not catch it and the old code posted the
                 *  front of a card several times a second. Each of those is a round trip on
                 *  office wifi, taken one at a time, which is where the wait came from. */
                val now = System.currentTimeMillis()

                if (!FieldRules.looksLikeZone(text)) {
                    if (now - lastPrintSentAt < PrintAgainAfter) return@addOnSuccessListener
                    lastPrintSentAt = now
                }

                lastSent = text
                onText(text)
            }
            .addOnFailureListener { e ->
                Log.d(TAG, "Recognition failed on a frame", e)
            }
            .addOnCompleteListener {
                busy.set(false)
                image.close()
            }
    }

    fun close() {
        running = false
        runCatching { recogniser.close() }
    }

    private fun worthSending(text: String): Boolean =
        text.count { it == '<' } >= MinimumChevrons ||
            PrintedNumber.containsMatchIn(text) ||
            /* Three long lines of zone characters is a zone whose chevrons were misread, which
               is the ordinary way this recogniser fails on OCR-B. Dropping those frames meant
               the one picture that could fill the whole form never left the tablet. */
            FieldRules.looksLikeZone(text)

    private companion object {
        const val TAG = "VmsMrz"

        /** Four, not one: a smudge can be read as a chevron, and the real zone has dozens. */
        const val MinimumChevrons = 4

        /** How long a frame showing only print waits before being asked about again. Long
            enough that holding a card up is not a stream of round trips, short enough that
            moving to a different card is answered while the officer is still holding it. */
        const val PrintAgainAfter = 2_000L

        /** 784 and fifteen digits in all, with whatever the recogniser put between them -
            the printed number groups with hyphens, and a hyphen is read as all sorts. */
        val PrintedNumber = Regex("""784\D{0,3}(?:\d\D{0,3}){12}""")
    }
}
