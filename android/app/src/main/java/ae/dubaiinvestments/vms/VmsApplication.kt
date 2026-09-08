package ae.dubaiinvestments.vms

import ae.dubaiinvestments.vms.api.VmsApi
import ae.dubaiinvestments.vms.api.VmsClient
import ae.dubaiinvestments.vms.auth.Auth
import ae.dubaiinvestments.vms.card.EmiratesIdReader
import ae.dubaiinvestments.vms.card.ToolkitEmiratesIdReader
import android.app.Application

/**
 * The three things the app is made of, held for the life of the process.
 *
 * A service locator rather than a dependency-injection framework. There are three
 * dependencies and one screen flow; Hilt would be more machinery than the problem has.
 *
 * All three are lazy. The toolkit in particular is expensive to start and touches native
 * code, and starting it in [onCreate] would put that on the launch path - so it starts
 * when the first reader query needs it, on a background thread, where a failure can be
 * shown on the screen instead of appearing as a crash on launch.
 */
class VmsApplication : Application() {

    val auth: Auth by lazy { Auth(this) }

    val api: VmsApi by lazy { VmsClient.create(auth) }

    val reader: EmiratesIdReader by lazy { ToolkitEmiratesIdReader(this) }
}
