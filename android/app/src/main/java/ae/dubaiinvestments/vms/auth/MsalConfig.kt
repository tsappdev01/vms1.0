package ae.dubaiinvestments.vms.auth

import ae.dubaiinvestments.vms.R
import android.content.Context
import org.json.JSONObject

/**
 * The client ID, read out of MSAL's own configuration file.
 *
 * The API scope is `api://<client id>/Visits.Write`, so the GUID is needed twice: once by
 * MSAL and once by us. Two copies of a GUID eventually disagree, and the symptom is a
 * token issued for the wrong audience and a 401 with nothing useful in it. So there is
 * one copy, in res/raw/auth_config.json, and this reads it.
 */
object MsalConfig {

    @Volatile
    private var cached: String? = null

    fun clientId(context: Context): String {
        cached?.let { return it }

        val json = context.resources.openRawResource(R.raw.auth_config)
            .bufferedReader()
            .use { it.readText() }

        val clientId = JSONObject(json).optString("client_id")
        require(clientId.isNotBlank()) {
            "res/raw/auth_config.json has no client_id. See android/README.md."
        }

        return clientId.also { cached = it }
    }
}
