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
 */
data class CardRead(
    val responseXml: String,
    val idNumber: String,
    val cardNumber: String?,
    val fullNameEnglish: String,
    val fullNameArabic: String?,
    val nationalityEnglish: String?,
    val gender: String?,
    val dateOfBirth: String?,
    val issueDate: String?,
    val expiryDate: String?,
    val photo: ByteArray?,
) {
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
