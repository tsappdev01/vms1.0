package ae.dubaiinvestments.vms.card

import ae.emiratesid.idcard.toolkit.CardReader
import ae.emiratesid.idcard.toolkit.Toolkit
import ae.emiratesid.idcard.toolkit.ToolkitException
import android.content.Context
import android.util.Base64
import android.util.Log
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock
import kotlinx.coroutines.withContext

/**
 * Reads an Emirates ID through ICP's toolkit and an ACS reader on USB.
 *
 * Two things shape this class.
 *
 * The toolkit is native and blocking, so every call goes to [Dispatchers.IO]. Called from
 * the main thread it would freeze the screen for the two or three seconds a read takes,
 * and Android would offer to close the app.
 *
 * It is also single-threaded per context and expensive to create, so there is one
 * instance and a [Mutex] around it. A second tap on Read Card while the first is still
 * going would otherwise reach the native layer twice at once.
 */
class ToolkitEmiratesIdReader(private val context: Context) : EmiratesIdReader {

    private companion object {
        const val TAG = "VmsCardReader"
    }

    private val gate = Mutex()

    private var toolkit: Toolkit? = null

    /** Remembered, so a misconfigured tablet gives the same clear answer every time
        rather than retrying a native initialisation on every tap. */
    private var initialisationError: String? = null

    private fun requireToolkit(): Toolkit {
        toolkit?.let { return it }
        initialisationError?.let { throw CardReadException(it) }

        try {
            val configDir = ToolkitConfig.ensureExtracted(context)
            val logDir = ToolkitConfig.logDirectory(context)
            val config = ToolkitConfig.build(context, configDir, logDir)

            Log.i(TAG, "Initialising the toolkit with:\n$config")

            /* The boolean is ICP's "validate the configuration" flag. Their sample passes
               true and so does the Windows build; false defers the complaint to the
               first read, which is a worse place to find out. */
            return Toolkit(true, config, context).also { toolkit = it }
        } catch (e: CardReadException) {
            initialisationError = e.message
            throw e
        } catch (e: ToolkitException) {
            initialisationError = e.message ?: "The toolkit refused to start (code ${e.code})."
            Log.e(TAG, "Toolkit initialisation failed, code ${e.code}", e)
            throw CardReadException(initialisationError!!, e)
        } catch (e: UnsatisfiedLinkError) {
            /* The ABI filter should prevent this, but an emulator or a repackaged APK can
               still get here, and the default message names a symbol rather than a cause. */
            initialisationError =
                "This tablet's processor is not one the toolkit supports. " +
                    "It needs arm64-v8a or armeabi-v7a."
            Log.e(TAG, "Native libraries missing for this ABI", e)
            throw CardReadException(initialisationError!!, e)
        }
    }

    override suspend fun state(): ReaderState = withContext(Dispatchers.IO) {
        gate.withLock {
            val kit = try {
                requireToolkit()
            } catch (e: CardReadException) {
                return@withLock ReaderState(detail = e.message ?: "The toolkit is not available.")
            }

            /* getToolkitVerison, with ICP's spelling. Kotlin's synthetic property keeps
               it, so this reads oddly on purpose - renaming it does not compile. */
            val version = runCatching { kit.toolkitVerison }.getOrNull()
            val licence = runCatching { kit.licenseExpiryDate }.getOrNull()

            try {
                /* Throws when there is no reader AND when there is a reader with no card
                   in it. Both are ordinary states at an idle desk, and the toolkit's own
                   message is the only thing that distinguishes them. */
                val reader = kit.readerWithEmiratesID
                ReaderState(
                    readerName = tidyReaderName(reader.name),
                    cardPresent = true,
                    detail = "Card detected. Ready to read.",
                    toolkitVersion = version,
                    licenceExpiry = licence,
                )
            } catch (e: ToolkitException) {
                ReaderState(
                    detail = e.message ?: "No reader found.",
                    toolkitVersion = version,
                    licenceExpiry = licence,
                )
            }
        }
    }

    override suspend fun read(requestId: String, onPhase: (String) -> Unit): CardRead =
        withContext(Dispatchers.IO) {
            gate.withLock {
                onPhase("Starting the toolkit…")
                val kit = requireToolkit()

                onPhase("Connecting to the card…")
                val reader: CardReader = try {
                    kit.readerWithEmiratesID.also { it.connect() }
                } catch (e: ToolkitException) {
                    throw CardReadException(
                        e.message ?: "The card could not be reached. Check it is seated chip-first.",
                        e,
                    )
                }

                try {
                    onPhase("Reading the chip…")

                    /* Non-modifiable data, the photograph and the address. Not the
                       modifiable data - occupation, sponsor and passport details are not
                       what a visitor log is for - and not the signature image, which is
                       in a format no client here renders and which the web app stopped
                       asking for. */
                    val data = reader.readPublicData(
                        requestId,
                        /* nonModifiable = */ true,
                        /* modifiable = */ false,
                        /* photography = */ true,
                        /* signatureImage = */ false,
                        /* address = */ true,
                    )

                    /* CardPublicData extends ToolkitResponse, and toXmlString() is the
                       signed Validation Gateway response - the whole document, which is
                       what the server verifies. Not getResponseDataElement(), which is
                       one element inside it. */
                    val xml = data.toXmlString()
                    if (xml.isNullOrBlank()) {
                        throw CardReadException(
                            "The card was read but the toolkit returned no signed response, " +
                                "so there is nothing the server can verify.",
                        )
                    }

                    val nm = data.nonModifiablePublicData

                    CardRead(
                        responseXml = xml,
                        idNumber = data.idNumber ?: "",
                        cardNumber = data.cardNumber,
                        fullNameEnglish = cleanName(nm?.fullNameEnglish),
                        fullNameArabic = cleanName(nm?.fullNameArabic).ifBlank { null },
                        nationalityEnglish = nm?.nationalityEnglish,
                        gender = nm?.gender,
                        dateOfBirth = nm?.dateOfBirth,
                        issueDate = nm?.issueDate,
                        expiryDate = nm?.expiryDate,
                        photo = decodePhoto(data.cardHolderPhoto),
                    )
                } catch (e: ToolkitException) {
                    Log.w(TAG, "Read failed, code ${e.code}", e)
                    throw CardReadException(e.message ?: "The card could not be read.", e)
                } finally {
                    // The card may already have been pulled out; that is not an error.
                    runCatching { reader.disconnect() }
                }
            }
        }

    /**
     * getCardHolderPhoto() hands back base64 text, not bytes - the toolkit's data model
     * is XML underneath and the photograph rides in it as a string. A photograph that
     * will not decode is not worth failing a check-in over; the desk still has the name.
     */
    private fun decodePhoto(base64: String?): ByteArray? {
        if (base64.isNullOrBlank()) return null
        return try {
            Base64.decode(base64, Base64.DEFAULT)
        } catch (e: IllegalArgumentException) {
            Log.w(TAG, "The photograph did not decode", e)
            null
        }
    }

    /**
     * The chip stores names as comma-delimited segments, most of them empty:
     * "NAYYAR JAWAID,,,,,ALI KHAN," is one person, not seven fields.
     */
    private fun cleanName(value: String?): String {
        if (value.isNullOrBlank()) return ""
        val joined = value.split(',').map(String::trim).filter(String::isNotEmpty).joinToString(" ")
        return joined.ifEmpty { value.trim() }
    }

    /** PC/SC hands back "0_ACS ACR39U ICC Reader 0" - an index, a name and a slot. */
    private fun tidyReaderName(name: String?): String? = name
        ?.trim()
        ?.replace(Regex("""^\d+_"""), "")
        ?.replace(Regex("""\s+\d+$"""), "")
        ?.ifBlank { name }
}
