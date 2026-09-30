package ae.dubaiinvestments.vms.settings

import android.content.Context
import java.security.MessageDigest
import java.security.SecureRandom
import javax.crypto.SecretKeyFactory
import javax.crypto.spec.PBEKeySpec
import kotlin.math.min

/** What a typed PIN was. */
sealed interface PinVerdict {
    data object Correct : PinVerdict

    /** Wrong, and how long the tablet will refuse to listen before the next try. */
    data class Wrong(val lockedForMillis: Long) : PinVerdict

    /** Not even looked at: the tablet is still counting down from earlier failures. */
    data class LockedOut(val remainingMillis: Long) : PinVerdict
}

/**
 * The PIN that stands between somebody holding this tablet and the API key.
 *
 * The key is a credential that works from anywhere on the internet, and the settings screen
 * both stores it and will show it. A reception tablet is handed to visitors and left on a
 * counter, so "you need the tablet" is not a control - this is the control.
 *
 * Three things it deliberately does:
 *
 * **It stores a hash, never the PIN.** PBKDF2-HMAC-SHA256 where the tablet's provider has it
 * and SHA-1 where it does not, a fresh 16-byte salt per PIN, [Iterations] rounds, off the main
 * thread.
 *
 * Worth being plain about what that buys, because it is less than it looks. Anyone who can read
 * this preferences file can read the API key sitting in it, so cracking the PIN gains them
 * nothing they did not already have. The hash is here so that a file pulled off the tablet does
 * not simply spell the PIN out - not because it has to withstand an attack. That is why
 * [Iterations] is tuned for the screen rather than against a cracking rig: what actually stops
 * a PIN being guessed is the lockout, and the lockout applies to the only attacker this guards
 * against, which is somebody typing into the tablet.
 *
 * **It slows guessing down, and remembers it had.** Ten thousand possibilities is an afternoon
 * for somebody typing, so wrong tries past [Patience] lock the field for a doubling delay up to
 * [MaximumLockMillis]. The count is on disk, not in memory: force-stopping the app is the
 * obvious way to reset a counter and it does not work here.
 *
 * **It has no back door.** There is no recovery code and no master PIN, because either would be
 * the thing worth attacking. A forgotten PIN is cleared by clearing the app's data in Android
 * settings - which also clears the API key and the server address, so the only way past the PIN
 * destroys what the PIN was protecting. That is the property worth having; write the PIN down
 * somewhere that is not the tablet.
 */
class DeskPin(context: Context) {

    /* The same preferences file as the server settings. One file, one thing to clear when a
       tablet is handed on or a PIN is forgotten. */
    private val prefs = context.getSharedPreferences("vms-settings", Context.MODE_PRIVATE)

    val isSet: Boolean
        get() = prefs.getString(KeyHash, null) != null && prefs.getString(KeySalt, null) != null

    /**
     * Sets or replaces the PIN.
     *
     * @return null when it was set, or why it was not, in the words to put on the screen.
     */
    fun set(pin: String): String? {
        validate(pin)?.let { return it }

        val salt = ByteArray(SaltBytes).also { SecureRandom().nextBytes(it) }
        val algorithm = available()

        prefs.edit()
            .putString(KeySalt, salt.toHex())
            .putString(KeyAlgorithm, algorithm)
            .putString(KeyHash, hash(pin, salt, algorithm).toHex())
            .remove(KeyFailures)
            .remove(KeyLockedUntil)
            .apply()

        return null
    }

    /** Removes the PIN, leaving the settings screen open to anyone. */
    fun clear() {
        prefs.edit()
            .remove(KeySalt)
            .remove(KeyAlgorithm)
            .remove(KeyHash)
            .remove(KeyFailures)
            .remove(KeyLockedUntil)
            .apply()
    }

    fun verify(pin: String): PinVerdict {
        val remaining = lockedForMillis()
        if (remaining > 0) return PinVerdict.LockedOut(remaining)

        val salt = prefs.getString(KeySalt, null)?.fromHex() ?: return PinVerdict.Wrong(0)
        val stored = prefs.getString(KeyHash, null)?.fromHex() ?: return PinVerdict.Wrong(0)

        /* The algorithm the PIN was set with, not whichever is best today. A tablet whose
           provider gains SHA-256 after a system update must still recognise the PIN it
           already has, and a stored name is how. */
        val algorithm = prefs.getString(KeyAlgorithm, null) ?: Sha1

        /* Constant time. The comparison is not the weak part of a four-digit PIN by any
           distance, but a byte-by-byte one is free to get wrong and free to get right. */
        if (MessageDigest.isEqual(hash(pin, salt, algorithm), stored)) {
            prefs.edit().remove(KeyFailures).remove(KeyLockedUntil).apply()
            return PinVerdict.Correct
        }

        val failures = prefs.getInt(KeyFailures, 0) + 1
        val lockFor = lockAfter(failures)

        prefs.edit()
            .putInt(KeyFailures, failures)
            .putLong(KeyLockedUntil, System.currentTimeMillis() + lockFor)
            .apply()

        return PinVerdict.Wrong(lockFor)
    }

    /** How long the field stays closed, or 0. */
    fun lockedForMillis(): Long {
        val until = prefs.getLong(KeyLockedUntil, 0L)
        val remaining = until - System.currentTimeMillis()

        /*  Wall clock, with one guard.
         *
         *  elapsedRealtime would be the honest choice for a duration, but it restarts at
         *  zero on a reboot, and rebooting a tablet is easier than waiting fifteen minutes.
         *  The wall clock survives that. What it does not survive is being changed, so a
         *  remaining time longer than any lock this class issues means the clock moved
         *  rather than that the lock is real, and the lock is dropped. The cost of getting
         *  that wrong is a few minutes off somebody's wait; the cost of not guarding it is
         *  a tablet locked until a date in 2031.
         */
        return if (remaining in 1..MaximumLockMillis) remaining else 0L
    }

    private fun hash(pin: String, salt: ByteArray, algorithm: String): ByteArray =
        SecretKeyFactory.getInstance(algorithm)
            .generateSecret(PBEKeySpec(pin.toCharArray(), salt, Iterations, KeyBits))
            .encoded

    /**
     * The best PBKDF2 this tablet actually has.
     *
     * Asked rather than assumed. `PBKDF2WithHmacSHA256` is documented from API 26 and this app
     * needs 28, but the provider on a given tablet is the provider on that tablet, and the
     * failure if it is absent is an exception thrown while somebody is setting a PIN. SHA-1 in
     * PBKDF2 is not a weakness of the same kind as SHA-1 in a signature - it is an HMAC, and
     * what stops a four-digit PIN being guessed here is the lockout, not the hash.
     */
    private fun available(): String =
        if (runCatching { SecretKeyFactory.getInstance(Sha256) }.isSuccess) Sha256 else Sha1

    companion object {
        const val MinimumDigits = 4
        const val MaximumDigits = 8

        /** Wrong tries allowed before the delays start. Four, so a fumbled entry and a
            retry cost nothing, and a fifth says somebody is trying combinations. */
        private const val Patience = 4

        private const val FirstLockMillis = 30_000L
        private const val MaximumLockMillis = 15 * 60_000L

        private const val SaltBytes = 16
        private const val KeyBits = 256

        /*  Measured, not picked.
         *
         *  On a fast desktop this costs about 60ms a hash, so a reception tablet is a few
         *  hundred milliseconds - felt as the settings screen taking a moment, which it can
         *  afford once per opening, and not as a lag. It is off the main thread either way.
         *
         *  Higher would buy resistance to offline cracking, which as above is not a thing
         *  this protects: the key is in the same file. */
        private const val Iterations = 60_000

        private const val Sha256 = "PBKDF2WithHmacSHA256"
        private const val Sha1 = "PBKDF2WithHmacSHA1"

        private const val KeySalt = "pinSalt"
        private const val KeyAlgorithm = "pinAlgorithm"
        private const val KeyHash = "pinHash"
        private const val KeyFailures = "pinFailures"
        private const val KeyLockedUntil = "pinLockedUntil"

        /** Nothing after the fourth try, then 30s doubling to a quarter of an hour. */
        fun lockAfter(failures: Int): Long {
            if (failures <= Patience) return 0L
            val doublings = min(failures - Patience - 1, 20)
            return min(FirstLockMillis shl doublings, MaximumLockMillis)
        }

        /**
         * Digits only, and [MinimumDigits] to [MaximumDigits] of them.
         *
         * Digits because the entry field is a number pad on a tablet that is mostly used
         * standing up. The range is stated here so the settings screen and this class cannot
         * disagree about what is acceptable.
         */
        fun validate(pin: String): String? = when {
            pin.length < MinimumDigits -> "The PIN must be at least $MinimumDigits digits."
            pin.length > MaximumDigits -> "The PIN can be at most $MaximumDigits digits."
            !pin.all(Char::isDigit) -> "The PIN must be digits only."
            else -> null
        }

        /** In words fit for the screen: "30 seconds", "2 minutes". */
        fun waitInWords(millis: Long): String {
            val seconds = ((millis + 999) / 1000).toInt()
            return when {
                seconds < 60 -> "$seconds second${if (seconds == 1) "" else "s"}"
                else -> {
                    val minutes = (seconds + 59) / 60
                    "$minutes minute${if (minutes == 1) "" else "s"}"
                }
            }
        }

        private fun ByteArray.toHex(): String = joinToString("") { "%02x".format(it) }

        private fun String.fromHex(): ByteArray? {
            if (length % 2 != 0) return null
            return runCatching {
                ByteArray(length / 2) { substring(it * 2, it * 2 + 2).toInt(16).toByte() }
            }.getOrNull()
        }
    }
}
