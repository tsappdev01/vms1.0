package ae.dubaiinvestments.vms.settings

import ae.dubaiinvestments.vms.BuildConfig
import android.content.Context
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import okhttp3.HttpUrl.Companion.toHttpUrlOrNull

/** Where this tablet sends its visits, and what it identifies itself with. */
data class ServerSettings(
    val baseUrl: String,
    val apiKey: String,
) {
    /** Just the host, for the top bar. A desk should be able to see at a glance which
        server it is talking to without opening anything. */
    val host: String
        get() = baseUrl.toHttpUrlOrNull()?.host ?: baseUrl
}

/**
 * The server address, kept on the tablet rather than compiled into the build.
 *
 * It was a build-time constant, which meant pointing a tablet at a different server was a
 * rebuild - and the person who needs to do that is standing in front of the tablet with a
 * reader plugged into it, not in front of Android Studio. So it is a setting now, with the
 * build-time value as its default: a fresh install still reaches the right server with
 * nothing typed.
 *
 * Stored in SharedPreferences, which `android:allowBackup="false"` keeps off cloud backups
 * and device-to-device transfers. The API key is in there in plain text; that is no weaker
 * than the constant it replaces, which anyone with the APK could read out of it, and it is
 * the reason the key is a tablet credential rather than a person's.
 */
class Settings(context: Context) {

    private val prefs = context.getSharedPreferences("vms-settings", Context.MODE_PRIVATE)

    private val _current = MutableStateFlow(read())
    val current: StateFlow<ServerSettings> = _current.asStateFlow()

    val value: ServerSettings get() = _current.value

    private fun read() = ServerSettings(
        baseUrl = prefs.getString(KeyBaseUrl, null)?.takeIf { it.isNotBlank() } ?: DefaultBaseUrl,
        apiKey = prefs.getString(KeyApiKey, null) ?: DefaultApiKey,
    )

    /**
     * Saves an address typed at the desk.
     *
     * @return null when it was saved, or why it was not - the caller puts that on the
     *   screen. A bad address is worth refusing here: saved, it would turn every later
     *   screen into a connection failure with nothing pointing back at this field.
     */
    fun save(baseUrl: String, apiKey: String): String? {
        val normalised = normalise(baseUrl) ?: return "That is not an address the tablet can use."

        prefs.edit()
            .putString(KeyBaseUrl, normalised)
            .putString(KeyApiKey, apiKey.trim())
            .apply()

        _current.value = read()
        return null
    }

    /** Back to the address this build was made with. */
    fun resetToDefault() {
        prefs.edit().remove(KeyBaseUrl).remove(KeyApiKey).apply()
        _current.value = read()
    }

    val isDefault: Boolean
        get() = value.baseUrl == DefaultBaseUrl && value.apiKey == DefaultApiKey

    companion object {
        private const val KeyBaseUrl = "baseUrl"
        private const val KeyApiKey = "apiKey"

        /** What the build was made with - see VMS_API_BASE_URL in gradle.properties. */
        val DefaultBaseUrl: String = normalise(BuildConfig.API_BASE_URL) ?: BuildConfig.API_BASE_URL
        val DefaultApiKey: String = BuildConfig.API_KEY

        /**
         * Turns what somebody typed into something Retrofit will take, or null.
         *
         * Two things are forgiven because they are what a person writes: a bare host with
         * no scheme, and a missing trailing slash. The second one is not cosmetic -
         * without it Retrofit drops the last path segment of the base URL, so
         * `https://host/vms` would silently become `https://host/`.
         */
        fun normalise(raw: String): String? {
            val text = raw.trim()
            if (text.isBlank()) return null

            val withScheme = if (text.contains("://")) text else "https://$text"
            val url = withScheme.toHttpUrlOrNull() ?: return null
            if (url.host.isBlank()) return null

            return withScheme.trimEnd('/') + "/"
        }
    }
}
