package ae.dubaiinvestments.vms

import ae.dubaiinvestments.vms.ui.VisitorViewModel
import ae.dubaiinvestments.vms.ui.VmsApp
import ae.dubaiinvestments.vms.ui.theme.VmsTheme
import android.os.Bundle
import android.view.WindowManager
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.lifecycle.viewmodel.compose.viewModel

/**
 * The only activity.
 *
 * `singleTop` in the manifest, so plugging the reader in brings this instance forward
 * instead of starting a second one on top of a half-finished check-in.
 */
class MainActivity : ComponentActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        /* A reception desk's tablet should not lock while somebody is at the counter, and
           the officer's hands are on a card and a visitor's passport, not the screen. */
        window.addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON)

        val app = application as VmsApplication

        setContent {
            VmsTheme {
                val model: VisitorViewModel = viewModel(factory = VisitorViewModel.Factory(app))
                VmsApp(viewModel = model, onSignIn = { model.signIn(this) })
            }
        }
    }
}
