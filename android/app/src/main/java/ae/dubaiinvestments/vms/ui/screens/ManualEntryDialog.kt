package ae.dubaiinvestments.vms.ui.screens

import ae.dubaiinvestments.vms.ui.FieldRules
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
 *
 * Every box is capped at the width of the column behind it, and the rules that decide
 * whether what was typed can be believed are [FieldRules] - the tablet's copy of the ones
 * the server applies when the save arrives, so the two cannot disagree about what a valid
 * Emirates ID number is.
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

                /*  The one typed field that blocks.
                 *
                 *  It is what a repeat visit is matched on, so a wrong one does not make a
                 *  bad record - it makes a second person. The fifteenth digit is a checksum
                 *  over the first fourteen and the officer is holding the card, so this is
                 *  worth the few seconds of retyping. */
                val idProblem = FieldRules.typedIdNumberProblem(draft.idNumber)

                OutlinedTextField(
                    value = draft.idNumber,
                    /* The officer types fifteen digits; the hyphens are put in here. Two of
                       them on a soft keyboard are two more chances for the number to come out
                       a character wrong, and the field never needed them. */
                    onValueChange = { entry ->
                        draft = draft.copy(
                            idNumber = FieldRules.retype(entry, draft.idNumber, FieldRules::groupIdNumber),
                        )
                    },
                    label = { Text("Emirates ID number") },
                    /* Said only once something has been typed: an empty required box is
                       already marked required, and one that turns red before it is touched
                       reads as a fault rather than as guidance. */
                    isError = draft.idNumber.isNotBlank() && idProblem != null,
                    supportingText = {
                        Text(
                            if (draft.idNumber.isNotBlank() && idProblem != null) idProblem
                            else "Type the digits - the hyphens are added for you.",
                        )
                    },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    singleLine = true,
                    modifier = Modifier.fillMaxWidth(),
                )

                OutlinedTextField(
                    value = draft.fullNameEnglish,
                    onValueChange = { draft = draft.copy(fullNameEnglish = it.take(FieldRules.Name)) },
                    label = { Text("Full name") },
                    singleLine = true,
                    modifier = Modifier.fillMaxWidth(),
                )

                OutlinedTextField(
                    value = draft.nationalityEnglish,
                    onValueChange = { draft = draft.copy(nationalityEnglish = it.take(FieldRules.CardField)) },
                    label = { Text("Nationality") },
                    singleLine = true,
                    modifier = Modifier.fillMaxWidth(),
                )

                OutlinedTextField(
                    value = draft.dateOfBirth,
                    onValueChange = { entry ->
                        draft = draft.copy(
                            dateOfBirth = FieldRules.retype(entry, draft.dateOfBirth, FieldRules::groupCardDate),
                        )
                    },
                    label = { Text("Date of birth") },
                    supportingText = { Text("Digits only - 13 12 1980 becomes 13/12/1980.") },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    singleLine = true,
                    modifier = Modifier.fillMaxWidth(),
                )

                /* A note, never a refusal - an unrecognised format must not be able to stop
                   a check-in, and an expired card is worth saying out loud but is still the
                   card the visitor has. */
                val expiryConcern = FieldRules.expiryConcern(draft.expiryDate)

                OutlinedTextField(
                    value = draft.expiryDate,
                    onValueChange = { entry ->
                        draft = draft.copy(
                            expiryDate = FieldRules.retype(entry, draft.expiryDate, FieldRules::groupCardDate),
                        )
                    },
                    label = { Text("Expiry date") },
                    isError = expiryConcern != null,
                    supportingText = {
                        Text(expiryConcern ?: "Required. Digits only - 25 08 2028 becomes 25/08/2028.")
                    },
                    keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                    singleLine = true,
                    modifier = Modifier.fillMaxWidth(),
                )

                OutlinedTextField(
                    value = draft.cardNumber,
                    onValueChange = { draft = draft.copy(cardNumber = it.take(FieldRules.CardNumber)) },
                    label = { Text("Card number (optional)") },
                    singleLine = true,
                    modifier = Modifier.fillMaxWidth(),
                )

                /*  Required, and it used to say "optional" here.
                 *
                 *  This box was the card's own mobile - a field that is empty on every card
                 *  tested at DIP - sitting next to a visit screen that required a different
                 *  mobile number. Two mobile fields at a reception desk, one of them marked
                 *  optional, is a desk that fills in the wrong one.
                 *
                 *  So there is one number now. What is typed here is the number the visit is
                 *  contacted on, it is carried to the visit screen, and the officer is not
                 *  asked for it twice. */
                val mobileConcern = FieldRules.mobileConcern(draft.mobile)

                OutlinedTextField(
                    value = draft.mobile,
                    onValueChange = { draft = draft.copy(mobile = it.take(FieldRules.ContactMobile)) },
                    label = { Text("Mobile number") },
                    isError = mobileConcern != null,
                    supportingText = {
                        Text(mobileConcern ?: "Required. How to reach the visitor.")
                    },
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
