package ae.dubaiinvestments.vms.ui.screens

import ae.dubaiinvestments.vms.BuildConfig
import ae.dubaiinvestments.vms.settings.Settings
import ae.dubaiinvestments.vms.ui.UiState
import ae.dubaiinvestments.vms.ui.parts.FieldRow
import ae.dubaiinvestments.vms.ui.parts.SectionCard
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Visibility
import androidx.compose.material.icons.filled.VisibilityOff
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.input.VisualTransformation
import androidx.compose.ui.unit.dp

/**
 * The one screen that is not part of a check-in.
 *
 * It exists because the server address used to be compiled in, and the person who needs to
 * change it is standing in front of the tablet with a reader plugged into it - not in front
 * of Android Studio. Moving a tablet between the Azure host and the on-premises one, or on
 * to a test server, is now typing rather than a rebuild.
 *
 * Deliberately only two fields. Everything else this app does is decided by the server or
 * by the card, and a settings screen that grows options is a settings screen reception has
 * to be trained on.
 */
@Composable
fun SettingsScreen(
    state: UiState,
    onTest: (String, String) -> Unit,
    onSave: (String, String) -> Unit,
    onResetToDefault: () -> Unit,
    onClose: () -> Unit,
) {
    var url by remember(state.server.baseUrl) { mutableStateOf(state.server.baseUrl) }
    var key by remember(state.server.apiKey) { mutableStateOf(state.server.apiKey) }
    var keyVisible by remember { mutableStateOf(false) }

    val changed = Settings.normalise(url) != state.server.baseUrl || key.trim() != state.server.apiKey

    Column(verticalArrangement = Arrangement.spacedBy(16.dp)) {

        SectionCard("Server") {
            Text(
                "Where this tablet sends its visits. It is set for you; change it only to " +
                    "point this tablet at a different VMS server.",
                style = MaterialTheme.typography.bodyMedium,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
            )

            OutlinedTextField(
                value = url,
                onValueChange = { url = it },
                label = { Text("Server address") },
                supportingText = { Text("Default: ${Settings.DefaultBaseUrl}") },
                singleLine = true,
                keyboardOptions = KeyboardOptions(
                    keyboardType = KeyboardType.Uri,
                    imeAction = ImeAction.Next,
                ),
                modifier = Modifier.fillMaxWidth(),
            )

            OutlinedTextField(
                value = key,
                onValueChange = { key = it },
                label = { Text("API key") },
                supportingText = {
                    Text(
                        "What the tablet identifies itself to the server with. The " +
                            "on-premises server asks for none - leave it empty for that one.",
                    )
                },
                singleLine = true,
                visualTransformation =
                    if (keyVisible) VisualTransformation.None else PasswordVisualTransformation(),
                trailingIcon = {
                    IconButton(onClick = { keyVisible = !keyVisible }) {
                        Icon(
                            if (keyVisible) Icons.Default.VisibilityOff else Icons.Default.Visibility,
                            contentDescription = if (keyVisible) "Hide the key" else "Show the key",
                            tint = MaterialTheme.colorScheme.onSurfaceVariant,
                        )
                    }
                },
                keyboardOptions = KeyboardOptions(imeAction = ImeAction.Done),
                modifier = Modifier.fillMaxWidth(),
            )

            state.settingsMessage?.let { message ->
                Text(
                    message,
                    style = MaterialTheme.typography.bodyMedium,
                    color = if (state.settingsFailed) {
                        MaterialTheme.colorScheme.error
                    } else {
                        MaterialTheme.colorScheme.onSurfaceVariant
                    },
                )
            }

            Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                OutlinedButton(
                    onClick = { onTest(url, key) },
                    enabled = state.settingsBusy == null,
                    modifier = Modifier.height(52.dp),
                ) {
                    if (state.settingsBusy != null) {
                        CircularProgressIndicator(modifier = Modifier.size(18.dp), strokeWidth = 2.dp)
                        Spacer(Modifier.size(10.dp))
                        Text(state.settingsBusy)
                    } else {
                        Text("Test connection")
                    }
                }

                Button(
                    onClick = { onSave(url, key) },
                    enabled = changed && state.settingsBusy == null,
                    modifier = Modifier
                        .weight(1f)
                        .height(52.dp),
                ) {
                    Text("Save")
                }
            }

            if (!state.serverIsDefault) {
                TextButton(onClick = onResetToDefault) { Text("Use the default address") }
            }
        }

        SectionCard("This tablet") {
            /* What to read out over the phone when a desk says it cannot reach the server.
               A version and a build type answer most of those calls before anyone drives
               to Dubai Investments Park. */
            FieldRow("App version", "${BuildConfig.VERSION_NAME} (${BuildConfig.VERSION_CODE})")
            FieldRow("Build", if (BuildConfig.DEBUG) "Debug" else "Release")
            FieldRow("Talking to", state.server.host)
            FieldRow("Sign-in", "Not used - the tablet identifies itself with the API key")
        }

        OutlinedButton(
            onClick = onClose,
            modifier = Modifier
                .fillMaxWidth()
                .height(56.dp),
        ) {
            Text("Back to the desk")
        }
    }
}
