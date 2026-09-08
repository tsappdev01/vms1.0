package ae.dubaiinvestments.vms.auth

import android.content.Context
import org.json.JSONObject

/**
 * Finds MSAL's configuration, and reads the client ID out of it.
 *
 * The API scope is `api://<client id>/Visits.Write`, so the GUID is needed twice: once by
 * MSAL and once by us. Two copies of a GUID eventually disagree, and the symptom is a
 * token issued for the wrong audience and a 401 with nothing useful in it. So there is
 * one copy, in res/raw/auth_config.json, and this reads it.
 *
 * The resource is looked up by name rather than referenced as `R.raw.auth_config`, and
 * that is the point of this object. `res/raw/auth_config.json` is not in the repository -
 * it identifies Dubai Investments' tenant - so a compile-time reference would mean the
 * app could not be built at all without it. With sign-in switched off nobody needs it, and
 * the build has no business insisting on a file it will not open.
 */
object MsalConfig {

    @Volatile
    private var cachedClientId: String? = null

    /** The `res/raw/auth_config.json` resource id, or 0 when this build has no such file. */
    fun resourceId(context: Context): Int =
        context.resources.getIdentifier("auth_config", "raw", context.packageName)

    fun isPresent(context: Context): Boolean = resourceId(context) != 0

    fun clientId(context: Context): String {
        cachedClientId?.let { return it }

        val id = resourceId(context)
        require(id != 0) {
            "This build has no res/raw/auth_config.json, so it cannot sign in. " +
                "Copy android/auth_config.template.json into place, or build with " +
                "-PVMS_AUTH_ENABLED=false. See android/README.md."
        }

        val json = context.resources.openRawResource(id).bufferedReader().use { it.readText() }

        val clientId = JSONObject(json).optString("client_id")
        require(clientId.isNotBlank() && !clientId.startsWith("REPLACE-")) {
            "res/raw/auth_config.json has no usable client_id. See android/README.md."
        }

        return clientId.also { cachedClientId = it }
    }
}
