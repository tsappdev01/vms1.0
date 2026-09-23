package ae.dubaiinvestments.vms.api

import ae.dubaiinvestments.vms.settings.ServerSettings
import ae.dubaiinvestments.vms.settings.Settings

/**
 * The API client for whatever server the tablet is currently pointed at.
 *
 * The address is a setting now, so the client cannot be built once and held: it is rebuilt
 * when - and only when - the settings change. Callers ask for [current] at the point of
 * use rather than holding the result, so an address changed on the settings screen takes
 * effect on the next call instead of on the next launch.
 */
class ApiProvider(private val settings: Settings) {

    private var built: Pair<ServerSettings, VmsApi>? = null

    @Synchronized
    fun current(): VmsApi {
        val wanted = settings.value

        built?.let { (usedSettings, api) -> if (usedSettings == wanted) return api }

        return VmsClient.create(wanted).also { built = wanted to it }
    }
}
