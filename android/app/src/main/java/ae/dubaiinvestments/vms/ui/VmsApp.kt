package ae.dubaiinvestments.vms.ui

import ae.dubaiinvestments.vms.BuildConfig
import ae.dubaiinvestments.vms.ui.parts.SectionCard
import ae.dubaiinvestments.vms.ui.screens.InsertCardScreen
import ae.dubaiinvestments.vms.ui.screens.ManualEntryDialog
import ae.dubaiinvestments.vms.ui.screens.SavedScreen
import ae.dubaiinvestments.vms.ui.screens.VisitDetailsScreen
import ae.dubaiinvestments.vms.ui.screens.VisitorInformationScreen
import ae.dubaiinvestments.vms.ui.theme.BrandNavy
import ae.dubaiinvestments.vms.ui.theme.OnBrandNavy
import ae.dubaiinvestments.vms.ui.theme.PagePadding
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.widthIn
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Close
import androidx.compose.material.icons.filled.Logout
import androidx.compose.material3.Button
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.material3.TopAppBarDefaults
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp

/**
 * The whole app: an app bar, four steps, and one place errors are shown.
 *
 * No navigation graph. The flow is linear, it always starts at Insert Card and it always
 * ends at Saved, and the back stack a navigation library would give us is one reception
 * does not want - a half-finished check-in should not be reachable behind the next
 * visitor's.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun VmsApp(viewModel: VisitorViewModel, onSignIn: () -> Unit) {
    val state by viewModel.state.collectAsState()
    var manualOpen by remember { mutableStateOf(false) }

    /* Runs only while the first screen is up, and is cancelled with it. A poll left
       running behind a finished check-in would keep the reader open and the tablet awake. */
    LaunchedEffect(state.step) {
        if (state.step == Step.InsertCard) viewModel.watchReader()
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = {
                    Column {
                        Text("Visitor Management", fontWeight = FontWeight.SemiBold)

                        /* The signed-in officer, or - while sign-in is off - the fact that
                           it is off. The web app carries the same note in the same corner,
                           for the same reason: this is temporary, and temporary states
                           become permanent by being invisible. */
                        val subtitle = if (BuildConfig.AUTH_ENABLED) state.signedInAs else "Sign-in is off"

                        subtitle?.let {
                            Text(
                                it,
                                style = MaterialTheme.typography.bodyMedium,
                                color = OnBrandNavy.copy(alpha = 0.75f),
                            )
                        }
                    }
                },
                actions = {
                    if (state.signedInAs != null) {
                        IconButton(onClick = viewModel::signOut) {
                            Icon(Icons.Default.Logout, contentDescription = "Sign out", tint = OnBrandNavy)
                        }
                    }
                },
                colors = TopAppBarDefaults.topAppBarColors(
                    containerColor = BrandNavy,
                    titleContentColor = OnBrandNavy,
                    actionIconContentColor = OnBrandNavy,
                ),
            )
        },
        containerColor = MaterialTheme.colorScheme.background,
    ) { insets ->
        Box(
            Modifier
                .fillMaxSize()
                .padding(insets),
            contentAlignment = Alignment.TopCenter,
        ) {
            Column(
                Modifier
                    /* A reception tablet is wide. A form stretched across all of it is
                       harder to read than the same form in a column, so the content is
                       capped and centred - which also makes portrait look deliberate. */
                    .widthIn(max = 720.dp)
                    .fillMaxSize()
                    .verticalScroll(rememberScrollState())
                    .padding(PagePadding),
                verticalArrangement = Arrangement.spacedBy(16.dp),
            ) {
                if (state.signInRequired) {
                    SignInPanel(state, onSignIn)
                    return@Column
                }

                StepRail(state.step)

                state.error?.let { message ->
                    ErrorBanner(message, viewModel::dismissError)
                }

                when (state.step) {
                    Step.InsertCard -> InsertCardScreen(
                        state = state,
                        onReadCard = { viewModel.readCard() },
                        onManualEntry = { manualOpen = true },
                    )

                    Step.VisitorInformation -> VisitorInformationScreen(
                        state = state,
                        onContinue = viewModel::toVisitDetails,
                        onEditManual = { manualOpen = true },
                    )

                    Step.VisitDetails -> VisitDetailsScreen(
                        state = state,
                        onEntity = viewModel::setEntity,
                        onHostQuery = viewModel::setHostQuery,
                        onSelectHost = viewModel::selectHost,
                        onSearchAllEntities = viewModel::setSearchAllEntities,
                        onPurpose = viewModel::setPurpose,
                        onPurposeOther = viewModel::setPurposeOther,
                        onBack = viewModel::backToVisitorInformation,
                        onSave = { viewModel.save() },
                    )

                    Step.Saved -> SavedScreen(state, viewModel::startOver)
                }
            }
        }
    }

    if (manualOpen) {
        ManualEntryDialog(
            initial = state.manual ?: ManualDraft(),
            onDismiss = { manualOpen = false },
            onConfirm = { draft ->
                manualOpen = false
                viewModel.useManualEntry(draft)
            },
        )
    }
}

/** Where reception is, in four words. Not a progress bar: the steps are named because
    "step 2 of 4" tells somebody nothing about what is on the screen. */
@Composable
private fun StepRail(current: Step) {
    Row(
        Modifier.fillMaxWidth(),
        horizontalArrangement = Arrangement.spacedBy(8.dp),
    ) {
        Step.entries.forEach { step ->
            val done = step.ordinal < current.ordinal
            val active = step == current

            Box(
                Modifier
                    .weight(1f)
                    .background(
                        when {
                            active -> MaterialTheme.colorScheme.primary
                            done -> MaterialTheme.colorScheme.primaryContainer
                            else -> MaterialTheme.colorScheme.surfaceVariant
                        },
                        RoundedCornerShape(8.dp),
                    )
                    .padding(vertical = 10.dp, horizontal = 8.dp),
                contentAlignment = Alignment.Center,
            ) {
                Text(
                    step.label,
                    style = MaterialTheme.typography.bodyMedium,
                    fontWeight = if (active) FontWeight.SemiBold else FontWeight.Normal,
                    color = when {
                        active -> MaterialTheme.colorScheme.onPrimary
                        done -> MaterialTheme.colorScheme.onPrimaryContainer
                        else -> MaterialTheme.colorScheme.onSurfaceVariant
                    },
                )
            }
        }
    }
}

private val Step.label: String
    get() = when (this) {
        Step.InsertCard -> "Card"
        Step.VisitorInformation -> "Visitor"
        Step.VisitDetails -> "Visit"
        Step.Saved -> "Done"
    }

/**
 * Errors are shown, not toasted.
 *
 * Every message here is either the server's own words or the toolkit's, and both are
 * written for somebody standing at a desk. A toast would take them away before the
 * officer had finished reading, and these are the messages that say what to do next.
 */
@Composable
private fun ErrorBanner(message: String, onDismiss: () -> Unit) {
    Row(
        Modifier
            .fillMaxWidth()
            .background(MaterialTheme.colorScheme.errorContainer, RoundedCornerShape(10.dp))
            .padding(start = 16.dp, top = 12.dp, bottom = 12.dp, end = 8.dp),
        verticalAlignment = Alignment.CenterVertically,
    ) {
        Text(
            message,
            style = MaterialTheme.typography.bodyMedium,
            color = MaterialTheme.colorScheme.onErrorContainer,
            modifier = Modifier.weight(1f),
        )
        IconButton(onClick = onDismiss) {
            Icon(
                Icons.Default.Close,
                contentDescription = "Dismiss",
                tint = MaterialTheme.colorScheme.onErrorContainer,
            )
        }
    }
}

@Composable
private fun SignInPanel(state: UiState, onSignIn: () -> Unit) {
    SectionCard("Sign in") {
        Text(
            "Sign in with your Dubai Investments account to record visits. " +
                "Your name goes on every entry you record.",
            style = MaterialTheme.typography.bodyMedium,
            color = MaterialTheme.colorScheme.onSurfaceVariant,
        )

        state.error?.let {
            Text(it, style = MaterialTheme.typography.bodyMedium, color = MaterialTheme.colorScheme.error)
        }

        Button(onClick = onSignIn, enabled = state.busy == null, modifier = Modifier.fillMaxWidth()) {
            Text(state.busy ?: "Sign in")
        }

        Spacer(Modifier.size(2.dp))
    }
}
