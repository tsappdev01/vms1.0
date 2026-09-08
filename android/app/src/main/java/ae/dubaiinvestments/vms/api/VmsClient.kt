package ae.dubaiinvestments.vms.api

import ae.dubaiinvestments.vms.BuildConfig
import ae.dubaiinvestments.vms.auth.Auth
import ae.dubaiinvestments.vms.auth.SignInRequired
import android.util.Log
import java.io.IOException
import java.net.SocketTimeoutException
import java.net.UnknownHostException
import java.util.concurrent.TimeUnit
import kotlinx.coroutines.runBlocking
import kotlinx.serialization.SerializationException
import kotlinx.serialization.json.Json
import okhttp3.Interceptor
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.HttpException
import retrofit2.Retrofit
import retrofit2.converter.kotlinx.serialization.asConverterFactory

/** Builds the API client, and translates its failures into something a desk can read. */
object VmsClient {

    private const val TAG = "VmsApi"

    private val json = Json {
        /* The server may grow a field before the tablets are updated; a new field must not
           break a check-in. */
        ignoreUnknownKeys = true
        explicitNulls = false
    }

    fun create(auth: Auth, baseUrl: String = BuildConfig.API_BASE_URL): VmsApi {
        val client = OkHttpClient.Builder()
            .addInterceptor(bearerToken(auth))
            .apply {
                if (BuildConfig.DEBUG) {
                    /* Headers and status only, never bodies. A body here is a signed card
                       response: an Emirates ID number, a date of birth and a photograph,
                       which have no business in logcat on a device at a reception desk. */
                    addInterceptor(
                        HttpLoggingInterceptor { Log.d(TAG, it) }
                            .apply { level = HttpLoggingInterceptor.Level.BASIC },
                    )
                }
            }
            /* A card read posts the signed XML with the photograph in it, over office
               wifi, so the write timeout is the generous one. */
            .connectTimeout(15, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .writeTimeout(60, TimeUnit.SECONDS)
            .build()

        return Retrofit.Builder()
            .baseUrl(baseUrl)
            .client(client)
            .addConverterFactory(json.asConverterFactory("application/json".toMediaType()))
            .build()
            .create(VmsApi::class.java)
    }

    /**
     * Attaches the access token when one can be had silently.
     *
     * When it cannot, the request goes out bare and the server answers 401 - which
     * [translate] turns into a sign-in prompt on the screen. The alternative, driving an
     * interactive sign-in from inside an interceptor, would mean launching an activity
     * from a background thread in the middle of a request.
     */
    private fun bearerToken(auth: Auth) = Interceptor { chain ->
        val token = try {
            runBlocking { auth.token(activity = null) }
        } catch (e: SignInRequired) {
            null
        } catch (e: Exception) {
            Log.w(TAG, "Could not get a token", e)
            null
        }

        val request = if (token == null) {
            chain.request()
        } else {
            chain.request().newBuilder().header("Authorization", "Bearer $token").build()
        }

        chain.proceed(request)
    }

    /**
     * Runs an API call and turns whatever went wrong into an [ApiException] whose message
     * is fit to put on the screen.
     *
     * The server writes its refusals for a person standing at a desk - "This read has
     * already been used, or took too long. Read the card again." - so those are passed
     * through as they are. Only the failures that happen below HTTP get a message of our
     * own, because OkHttp's are about sockets.
     */
    suspend fun <T> call(block: suspend () -> T): T =
        try {
            block()
        } catch (e: HttpException) {
            throw ApiException(describe(e), e.code())
        } catch (e: UnknownHostException) {
            throw ApiException("The server could not be reached. Check the tablet is on the office network.")
        } catch (e: SocketTimeoutException) {
            throw ApiException("The server did not answer in time. Try again.")
        } catch (e: IOException) {
            throw ApiException("The connection to the server failed. Check the tablet is on the office network.")
        } catch (e: SerializationException) {
            /* A 200 whose body is not what this build expects - an IIS interstitial, or a
               server newer than the tablet. Worth its own message: it is not a network
               fault and retrying will not help. */
            Log.w(TAG, "Could not read the server's answer", e)
            throw ApiException("The server's answer could not be read. The tablet may need updating.")
        }

    private fun describe(e: HttpException): String {
        if (e.code() == 401) return "Your sign-in has expired. Sign in again."
        if (e.code() == 403) return "Your account is not allowed to record visits. Ask IT for the Reception role."

        /* ProblemDetails, if that is what came back. An HTML error page from IIS is the
           other possibility, and parsing that as JSON throws - hence the runCatching. */
        val detail = runCatching {
            e.response()?.errorBody()?.string()?.takeIf { it.isNotBlank() }?.let {
                json.decodeFromString<ProblemDetails>(it)
            }
        }.getOrNull()

        detail?.detail?.takeIf { it.isNotBlank() }?.let { return it }
        detail?.title?.takeIf { it.isNotBlank() }?.let { return it }

        return "The server refused the request (${e.code()})."
    }
}
