package ae.dubaiinvestments.vms.ui.screens

import ae.dubaiinvestments.vms.ui.ManualDraft
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.ui.unit.dp

/**
 * Typing the details in, for when the chip will not read.
 *
 * There is always a way to record a visit. A cracked chip or a reader that has died mid
 * shift cannot be a reason to turn a visitor away, so this exists - and the record it
 * makes is marked "Manual" in the report, which is the honest outcome: the desk saw the
 * card, the system did not.
 *
 * Only the fields the server accepts by hand. There is no point offering to type a
 * photograph or a place of birth that the visit log does not use.
 */
@Composable
fun ManualEntryDialog(
    initial: ManualDraft,
    onDismiss: () -> Unit,
    onConfirm: (ManualDraft) -> Unit,
) {
    var draft by remember { mutableStateOf(initial) }

    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text("Enter the details by hand") },
        text = {
            Column(
                Modifier
                    .fillMaxWidth()
                    .verticalScroll(rememberScrollState()),
                verticalArrangement = Arrangement.spacedBy(10.dp),
            ) {
                Text(
                    "Copy them from the front of the card. The visit is recorded as a " +
                        "manual entry so the report shows it was not read from the chip.",
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                )

                OutlinedTextField(
                    value = draft.idNumber,
                    onValueChange = { draft = draft.copy(idNumber = it) },
                    label = { Text("Emirates ID number") },
                    supportingText = { Text("784-…-…-…") },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    singleLine = true,
                    modifier = Modifier.fillMaxWidth(),
                )

                OutlinedTextField(
                    value = draft.fullNameEnglish,
                    onValueChange = { draft = draft.copy(fullNameEnglish = it) },
                    label = { Text("Full name") },
                    singleLine = true,
                    modifier = Modifier.fillMaxWidth(),
                )

                OutlinedTextField(
                    value = draft.nationalityEnglish,
                    onValueChange = { draft = draft.copy(nationalityEnglish = it) },
                    label = { Text("Nationality") },
                    singleLine = true,
                    modifier = Modifier.fillMaxWidth(),
                )

                OutlinedTextField(
                    value = draft.dateOfBirth,
                    onValueChange = { draft = draft.copy(dateOfBirth = it) },
                    label = { Text("Date of birth") },
                    supportingText = { Text("As printed on the card") },
                    singleLine = true,
                    modifier = Modifier.fillMaxWidth(),
                )

                OutlinedTextField(
                    value = draft.expiryDate,
                    onValueChange = { draft = draft.copy(expiryDate = it) },
                    label = { Text("Expiry date") },
                    singleLine = true,
                    modifier = Modifier.fillMaxWidth(),
                )

                OutlinedTextField(
                    value = draft.cardNumber,
                    onValueChange = { draft = draft.copy(cardNumber = it) },
                    label = { Text("Card number (optional)") },
                    singleLine = true,
                    modifier = Modifier.fillMaxWidth(),
                )

                OutlinedTextField(
                    value = draft.mobile,
                    onValueChange = { draft = draft.copy(mobile = it) },
                    label = { Text("Mobile (optional)") },
                    keyboardOptions = KeyboardOptions(
                        keyboardType = KeyboardType.Phone,
                        imeAction = ImeAction.Done,
                    ),
                    singleLine = true,
                    modifier = Modifier.fillMaxWidth(),
                )
            }
        },
        confirmButton = {
            TextButton(
                onClick = { onConfirm(draft) },
                enabled = draft.isUsable,
            ) {
                Text("Use these details")
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss) { Text("Cancel") }
        },
    )
}
