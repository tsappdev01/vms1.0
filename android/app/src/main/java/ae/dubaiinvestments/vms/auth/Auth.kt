package ae.dubaiinvestments.vms.auth

import ae.dubaiinvestments.vms.R
import android.app.Activity
import android.content.Context
import android.util.Log
import com.microsoft.identity.client.AcquireTokenParameters
import com.microsoft.identity.client.AcquireTokenSilentParameters
import com.microsoft.identity.client.AuthenticationCallback
import com.microsoft.identity.client.IAccount
import com.microsoft.identity.client.IAuthenticationResult
import com.microsoft.identity.client.IPublicClientApplication
import com.microsoft.identity.client.ISingleAccountPublicClientApplication
import com.microsoft.identity.client.PublicClientApplication
import com.microsoft.identity.client.SilentAuthenticationCallback
import com.microsoft.identity.client.exception.MsalException
import com.microsoft.identity.client.exception.MsalUiRequiredException
import kotlinx.coroutines.suspendCancellableCoroutine
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock
import kotlin.coroutines.resume
import kotlin.coroutines.resumeWithException

/**
 * Entra ID sign-in, and the access token for the VMS API.
 *
 * Single-account on purpose. A reception tablet is one desk with one signed-in officer at
 * a time, and the multiple-account client would offer an account picker on every token
 * request - a prompt with a visitor waiting.
 *
 * Silent first, always. A token lives in MSAL's cache and refreshes without a prompt, so
 * an officer signs in at the start of a shift and not again. Only a genuine
 * [MsalUiRequiredException] sends the desk back to a prompt.
 *
 * Every MSAL entry point used here is the callback form. The synchronous ones throw if
 * they are called on the main thread, and the token is fetched from whatever thread OkHttp
 * happens to be on - so the synchronous API would work in testing and fail in the hands
 * of reception.
 */
class Auth(private val context: Context) {

    private companion object {
        const val TAG = "VmsAuth"
    }

    /** The API's own scope, from the Entra app registration's Expose an API. Not a Graph
        scope: this token is for our server, and the server checks the audience. */
    private val scopes: List<String>
        get() = listOf("api://${MsalConfig.clientId(context)}/Visits.Write")

    private val creation = Mutex()
    private var client: ISingleAccountPublicClientApplication? = null

    private suspend fun client(): ISingleAccountPublicClientApplication {
        client?.let { return it }

        return creation.withLock {
            client?.let { return@withLock it }

            val created: ISingleAccountPublicClientApplication = suspendCancellableCoroutine { cont ->
                PublicClientApplication.createSingleAccountPublicClientApplication(
                    context,
                    R.raw.auth_config,
                    object : IPublicClientApplication.ISingleAccountApplicationCreatedListener {
                        override fun onCreated(application: ISingleAccountPublicClientApplication) =
                            cont.resume(application)

                        override fun onError(exception: MsalException) =
                            cont.resumeWithException(exception)
                    },
                )
            }

            created.also { client = it }
        }
    }

    /** The signed-in user, or null. Used for the name in the top bar. */
    suspend fun currentAccountName(): String? = cachedAccount()?.username

    /**
     * A token, without a prompt if one can be had that way.
     *
     * @param activity needed only if the desk has to be asked. Null means "silent or
     *   nothing", which is what a token fetch on a background thread wants.
     */
    suspend fun token(activity: Activity?): String {
        silentToken()?.let { return it }

        if (activity == null) {
            /* Our own type rather than one of MSAL's: the caller's job is to put the
               sign-in button in front of the officer, and that does not need an OAuth
               error code attached to it. */
            throw SignInRequired()
        }

        return interactiveToken(activity)
    }

    /** True if a token can be had without troubling the desk. */
    suspend fun isSignedIn(): Boolean = silentToken() != null

    private suspend fun cachedAccount(): IAccount? {
        val client = client()

        return suspendCancellableCoroutine { cont ->
            client.getCurrentAccountAsync(object : ISingleAccountPublicClientApplication.CurrentAccountCallback {
                override fun onAccountLoaded(activeAccount: IAccount?) = cont.resume(activeAccount)

                override fun onAccountChanged(priorAccount: IAccount?, currentAccount: IAccount?) = Unit

                override fun onError(exception: MsalException) {
                    Log.w(TAG, "Could not read the cached account", exception)
                    cont.resume(null)
                }
            })
        }
    }

    private suspend fun silentToken(): String? {
        val client = client()
        val account = cachedAccount() ?: return null

        return suspendCancellableCoroutine { cont ->
            val parameters = AcquireTokenSilentParameters.Builder()
                .forAccount(account)
                .fromAuthority(account.authority)
                .withScopes(scopes)
                .withCallback(object : SilentAuthenticationCallback {
                    override fun onSuccess(result: IAuthenticationResult) = cont.resume(result.accessToken)

                    override fun onError(exception: MsalException) {
                        /* Null either way, so the caller falls through to a prompt - but a
                           refusal that is not "we need the user" is logged, because a
                           revoked account or a conditional-access policy is not something
                           the prompt will fix, and the log is where that shows up. */
                        if (exception !is MsalUiRequiredException) {
                            Log.w(TAG, "Silent token failed", exception)
                        }
                        cont.resume(null)
                    }
                })
                .build()

            client.acquireTokenSilentAsync(parameters)
        }
    }

    private suspend fun interactiveToken(activity: Activity): String {
        val client = client()

        return suspendCancellableCoroutine { cont ->
            val parameters = AcquireTokenParameters.Builder()
                .startAuthorizationFromActivity(activity)
                .withScopes(scopes)
                .withCallback(object : AuthenticationCallback {
                    override fun onSuccess(result: IAuthenticationResult) = cont.resume(result.accessToken)
                    override fun onError(exception: MsalException) = cont.resumeWithException(exception)
                    override fun onCancel() = cont.resumeWithException(SignInCancelled())
                })
                .build()

            client.acquireTokenAsync(parameters)
        }
    }

    suspend fun signOut() {
        val client = client()

        suspendCancellableCoroutine { cont ->
            client.signOut(object : ISingleAccountPublicClientApplication.SignOutCallback {
                override fun onSignOut() = cont.resume(Unit)

                override fun onError(exception: MsalException) {
                    Log.w(TAG, "Sign-out failed", exception)
                    cont.resume(Unit)
                }
            })
        }
    }
}

/** Nobody is signed in, and only the desk can fix that. */
class SignInRequired : Exception("Sign in to record a visit.")

/** The officer closed the sign-in page. Not a failure worth a red banner. */
class SignInCancelled : Exception("Sign-in was cancelled.")
