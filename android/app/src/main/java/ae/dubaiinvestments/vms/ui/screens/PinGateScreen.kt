package ae.dubaiinvestments.vms.ui.screens

import ae.dubaiinvestments.vms.settings.DeskPin
import ae.dubaiinvestments.vms.ui.UiState
import ae.dubaiinvestments.vms.ui.parts.SectionCard
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.text.KeyboardActions
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.Button
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.unit.dp

/**
 * What stands in front of the settings screen once a PIN has been set.
 *
 * The screen behind it holds the API key - a credential that works from anywhere on the
 * internet and that the field will show on request. Everything else this app does is meant
 * to be done by whoever is holding it; this is the one thing that is not.
 *
 * No key pad drawn here. `KeyboardType.NumberPassword` is the tablet's own, which is large,
 * familiar, and already the right shape.
 */
@Composable
fun PinGateScreen(
    state: UiState,
    onSubmit: (String) -> Unit,
    onClose: () -> Unit,
) {
    var typed by remember { mutableStateOf("") }

    val lockedOut = state.pinLockedSeconds > 0

    /* Cleared the moment a lockout starts, so the field is not sitting there full of a PIN
       that has already been refused, inviting the same button to be pressed again. */
    LaunchedEffect(lockedOut) {
        if (lockedOut) typed = ""
    }

    Column(verticalArrangement = Arrangement.spacedBy(16.dp)) {
        SectionCard("Settings are protected") {
            Text(
                "This screen holds the key this tablet identifies itself to the server with. " +
                    "Enter the PIN to open it.",
                style = MaterialTheme.typography.bodyMedium,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
            )

            OutlinedTextField(
                value = typed,
                onValueChange = { entry ->
                    typed = entry.filter(Char::isDigit).take(DeskPin.MaximumDigits)
                },
                label = { Text("PIN") },
                singleLine = true,
                enabled = !lockedOut,
                isError = state.pinFailed,
                visualTransformation = PasswordVisualTransformation(),
                keyboardOptions = KeyboardOptions(
                    keyboardType = KeyboardType.NumberPassword,
                    imeAction = ImeAction.Done,
                ),
                keyboardActions = KeyboardActions(onDone = { onSubmit(typed) }),
                modifier = Modifier.fillMaxWidth(),
            )

            /* The countdown, not a bare refusal. "Wrong" with a dead button and no reason is
               the shape of a broken app; a number going down is a tablet doing its job. */
            val message = if (lockedOut) {
                "Too many tries. Try again in ${state.pinLockedSeconds} " +
                    "second${if (state.pinLockedSeconds == 1) "" else "s"}."
            } else {
                state.pinMessage
            }

            message?.let {
                Text(
                    it,
                    style = MaterialTheme.typography.bodyMedium,
                    color = if (state.pinFailed || lockedOut) {
                        MaterialTheme.colorScheme.error
                    } else {
                        MaterialTheme.colorScheme.onSurfaceVariant
                    },
                )
            }

            Button(
                onClick = { onSubmit(typed) },
                enabled = !lockedOut && typed.length >= DeskPin.MinimumDigits,
                modifier = Modifier
                    .fillMaxWidth()
                    .height(56.dp),
            ) {
                Text("Open settings")
            }

            Text(
                "Forgotten it? There is no way to recover it. Clear the app's data in " +
                    "Android settings, which also clears the server address and the key, then " +
                    "set the tablet up again.",
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
            )
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
