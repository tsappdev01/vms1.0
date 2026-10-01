package ae.dubaiinvestments.vms.ui.screens

import ae.dubaiinvestments.vms.BuildConfig
import ae.dubaiinvestments.vms.settings.DeskPin
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
import androidx.compose.material3.Switch
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.input.KeyboardCapitalization
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
 * Deliberately small. Everything else this app does is decided by the server or by the
 * card, and a settings screen that grows options is a settings screen reception has to be
 * trained on.
 *
 * The exception is the PIN, which is here because the API key is here. That key reaches the
 * server from anywhere on the internet, and the field above will show it to whoever asks -
 * so on a tablet that sits on a counter, this screen is the one thing in the app worth
 * locking. See [ae.dubaiinvestments.vms.settings.DeskPin] for what the lock is worth.
 */
@Composable
fun SettingsScreen(
    state: UiState,
    onTest: (String, String) -> Unit,
    onSave: (String, String) -> Unit,
    onResetToDefault: () -> Unit,
    onOfflineToolkit: (Boolean) -> Unit,
    onSetPin: (current: String, new: String, confirm: String) -> Unit,
    onRemovePin: (current: String) -> Unit,
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
                /*  Password, and autocorrect off, both stated rather than left to the
                 *  default.
                 *
                 *  This is sixty-four random characters typed on a soft keyboard. A field the
                 *  IME believes to be prose will helpfully capitalise, suggest and substitute
                 *  in it, and the result is a key that looks right on screen, is wrong by one
                 *  character, and produces a 401 that says only that the server did not accept
                 *  it. Nothing anywhere points at the keyboard.
                 *
                 *  KeyboardType.Password is the one that tells every IME to keep out. */
                keyboardOptions = KeyboardOptions(
                    keyboardType = KeyboardType.Password,
                    autoCorrect = false,
                    capitalization = KeyboardCapitalization.None,
                    imeAction = ImeAction.Done,
                ),
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

        SectionCard("Card reading") {
            Row(verticalAlignment = Alignment.CenterVertically) {
                Column(Modifier.weight(1f)) {
                    Text("Read cards without ICP's gateway", style = MaterialTheme.typography.bodyLarge)
                    Text(
                        if (state.server.offlineToolkit) {
                            "On. The toolkit is kept off the internet, so a read needs nothing " +
                                "but the reader. The visit records as unverified, which is what " +
                                "the report already shows for every read from this tablet."
                        } else {
                            "Off. Reading a card calls ICP's Validation Gateway over the " +
                                "internet. On a tablet that cannot reach it, every read fails " +
                                "with \"failed to get response from server\" (toolkit code 233)."
                        },
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                    )
                }
                Switch(
                    checked = state.server.offlineToolkit,
                    onCheckedChange = onOfflineToolkit,
                )
            }
        }

        PinCard(state = state, onSetPin = onSetPin, onRemovePin = onRemovePin)

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

/**
 * Setting, changing and removing the PIN.
 *
 * The current PIN is asked for even though this screen is already open. The case it guards is
 * not somebody who got past the gate - it is a settings screen left open on the counter while
 * reception answers the phone, which is how a tablet is actually reached.
 */
@Composable
private fun PinCard(
    state: UiState,
    onSetPin: (String, String, String) -> Unit,
    onRemovePin: (String) -> Unit,
) {
    /* Emptied once the view model has something to say about what was typed - a PIN set, a
       PIN changed, a current PIN refused. Three boxes still holding digits under a message
       saying what became of them is an invitation to press the button a second time. */
    val settled = state.pinMessage != null && !state.pinFailed

    var current by remember(state.pinIsSet, settled) { mutableStateOf("") }
    var fresh by remember(state.pinIsSet, settled) { mutableStateOf("") }
    var confirm by remember(state.pinIsSet, settled) { mutableStateOf("") }

    SectionCard(if (state.pinIsSet) "Settings PIN" else "No settings PIN") {
        if (state.pinIsSet) {
            Text(
                "Settings ask for this PIN each time they are opened. It is not the tablet's " +
                    "screen lock and it protects nothing else in the app - only this screen.",
                style = MaterialTheme.typography.bodyMedium,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
            )
        } else {
            /* Stated as the exposure rather than as a suggestion. "Set a PIN for extra
               security" is advice nobody takes; what is true is that the key is readable. */
            Text(
                "Anyone who picks this tablet up can open this screen and read the API key, " +
                    "which reaches the server from anywhere. Set a PIN to stop that.",
                style = MaterialTheme.typography.bodyMedium,
                color = MaterialTheme.colorScheme.error,
            )
        }

        if (state.pinIsSet) {
            PinField(
                value = current,
                onValueChange = { current = it },
                label = "Current PIN",
                imeAction = ImeAction.Next,
            )
        }

        PinField(
            value = fresh,
            onValueChange = { fresh = it },
            label = if (state.pinIsSet) "New PIN" else "PIN",
            imeAction = ImeAction.Next,
        )

        PinField(
            value = confirm,
            onValueChange = { confirm = it },
            label = "Enter it again",
            imeAction = ImeAction.Done,
        )

        Text(
            "${DeskPin.MinimumDigits} to ${DeskPin.MaximumDigits} digits. There is no way to " +
                "recover it: a forgotten PIN is cleared by clearing the app's data in Android " +
                "settings, which clears the server address and the key with it. Write it down " +
                "somewhere that is not this tablet.",
            style = MaterialTheme.typography.bodySmall,
            color = MaterialTheme.colorScheme.onSurfaceVariant,
        )

        state.pinMessage?.let { message ->
            Text(
                message,
                style = MaterialTheme.typography.bodyMedium,
                color = if (state.pinFailed) {
                    MaterialTheme.colorScheme.error
                } else {
                    MaterialTheme.colorScheme.onSurfaceVariant
                },
            )
        }

        Button(
            onClick = { onSetPin(current, fresh, confirm) },
            enabled = fresh.length >= DeskPin.MinimumDigits &&
                confirm.isNotEmpty() &&
                (!state.pinIsSet || current.length >= DeskPin.MinimumDigits),
            modifier = Modifier
                .fillMaxWidth()
                .height(52.dp),
        ) {
            Text(if (state.pinIsSet) "Change the PIN" else "Set the PIN")
        }

        if (state.pinIsSet) {
            TextButton(
                onClick = { onRemovePin(current) },
                enabled = current.length >= DeskPin.MinimumDigits,
            ) {
                Text("Remove the PIN (enter the current one above)")
            }
        }
    }
}

/** One PIN box. Digits only, masked, and on the tablet's number pad - the three things
    every field in this card wants and none of them worth repeating three times. */
@Composable
private fun PinField(
    value: String,
    onValueChange: (String) -> Unit,
    label: String,
    imeAction: ImeAction,
) {
    OutlinedTextField(
        value = value,
        onValueChange = { entry ->
            onValueChange(entry.filter(Char::isDigit).take(DeskPin.MaximumDigits))
        },
        label = { Text(label) },
        singleLine = true,
        visualTransformation = PasswordVisualTransformation(),
        keyboardOptions = KeyboardOptions(
            keyboardType = KeyboardType.NumberPassword,
            imeAction = imeAction,
        ),
        modifier = Modifier.fillMaxWidth(),
    )
}
