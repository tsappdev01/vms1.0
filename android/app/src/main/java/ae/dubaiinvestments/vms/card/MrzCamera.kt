package ae.dubaiinvestments.vms.card

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
                /* Line by line rather than result.text, because the zone is three lines and
                   the server looks for three lines. Block order is not reading order for a
                   card held at an angle, so each line goes up on its own and the server tries
                   every run of three. */
                val lines = result.textBlocks
                    .flatMap { it.lines }
                    .map { it.text.trim() }
                    .filter { it.isNotEmpty() }

                if (lines.isEmpty()) return@addOnSuccessListener

                val text = lines.joinToString("\n")

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
        text.count { it == '<' } >= MinimumChevrons || PrintedNumber.containsMatchIn(text)

    private companion object {
        const val TAG = "VmsMrz"

        /** Four, not one: a smudge can be read as a chevron, and the real zone has dozens. */
        const val MinimumChevrons = 4

        /** 784 and fifteen digits in all, with whatever the recogniser put between them -
            the printed number groups with hyphens, and a hyphen is read as all sorts. */
        val PrintedNumber = Regex("""784\D{0,3}(?:\d\D{0,3}){12}""")
    }
}
