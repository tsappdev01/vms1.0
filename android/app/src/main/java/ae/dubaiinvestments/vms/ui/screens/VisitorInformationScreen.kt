package ae.dubaiinvestments.vms.ui.screens

import ae.dubaiinvestments.vms.ui.UiState
import ae.dubaiinvestments.vms.ui.parts.FieldRow
import ae.dubaiinvestments.vms.ui.parts.Fold
import ae.dubaiinvestments.vms.ui.parts.SectionCard
import ae.dubaiinvestments.vms.ui.parts.VisitorPhoto
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.size
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Edit
import androidx.compose.material3.AssistChip
import androidx.compose.material3.AssistChipDefaults
import androidx.compose.material3.Button
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

/**
 * Step two. Who is standing at the desk.
 *
 * Read-only when it came off the chip, because it did: correcting a card read on the
 * tablet would produce a record that says "read from the card" and does not match the
 * card. A wrong name means the card was misread, and the answer to that is to read it
 * again - not to type over it.
 */
@Composable
fun VisitorInformationScreen(
    state: UiState,
    onContinue: () -> Unit,
    onEditManual: () -> Unit,
) {
    val card = state.card
    val manual = state.manual

    Column(verticalArrangement = Arrangement.spacedBy(16.dp)) {

        SectionCard {
            Row(verticalAlignment = Alignment.Top) {
                VisitorPhoto(card?.photo)
                Spacer(Modifier.size(18.dp))
                Column(Modifier.weight(1f), verticalArrangement = Arrangement.spacedBy(6.dp)) {
                    Text(state.visitorName, style = MaterialTheme.typography.headlineSmall)

                    card?.fullNameArabic?.let {
                        Text(it, style = MaterialTheme.typography.bodyLarge)
                    }

                    Text(
                        state.visitorIdNumber ?: "—",
                        style = MaterialTheme.typography.bodyLarge,
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                    )

                    Spacer(Modifier.size(2.dp))

                    AssistChip(
                        onClick = {},
                        enabled = false,
                        label = {
                            Text(
                                if (state.isManualEntry) "Entered by hand" else "Read from the card",
                            )
                        },
                        colors = AssistChipDefaults.assistChipColors(
                            disabledLabelColor = MaterialTheme.colorScheme.onSurfaceVariant,
                        ),
                    )
                }
            }

            if (state.isManualEntry) {
                Button(onClick = onEditManual, modifier = Modifier.fillMaxWidth()) {
                    Icon(Icons.Default.Edit, contentDescription = null, modifier = Modifier.size(18.dp))
                    Spacer(Modifier.size(8.dp))
                    Text("Change these details")
                }
            }
        }

        /*  Folded, because reception needs the name and the face and nothing else until
         *  somebody asks - but everything the server parses out of the card is in here when
         *  they do. It used to stop at six rows while the record carried twenty-three, which
         *  is how a mobile number sitting on the chip came to be typed in by hand. */
        Fold("Card details") {
            FieldRow("Name (Arabic)", card?.fullNameArabic)
            FieldRow("Title", card?.titleEnglish)
            FieldRow("Nationality", card?.nationalityEnglish ?: manual?.nationalityEnglish)
            FieldRow("Date of birth", card?.dateOfBirth ?: manual?.dateOfBirth)
            FieldRow("Place of birth", card?.placeOfBirthEnglish)
            FieldRow("Gender", card?.gender)
            FieldRow("Card number", card?.cardNumber ?: manual?.cardNumber)
            FieldRow("Card type", card?.idType)
            FieldRow("Issued", card?.issueDate)
            FieldRow("Expires", card?.expiryDate ?: manual?.expiryDate)
            if (manual != null) FieldRow("Mobile", manual.mobile)
        }

        /*  Its own fold, and only when a card was read.
         *
         *  The chip is asked for the address on every read and the server has always stored
         *  it; the tablet threw all of it away. It is the one part of a card a reception desk
         *  genuinely does not need in front of it, which is why it is folded separately rather
         *  than mixed into the rows above - but "did not need to see" was never a reason not
         *  to have read it. */
        if (card?.hasHomeAddress == true) {
            Fold("Home address, from the card") {
                FieldRow("Emirate", card.addressEmirate)
                FieldRow("City", card.addressCity)
                FieldRow("Area", card.addressArea)
                FieldRow("Street", card.addressStreet)
                FieldRow("Building", card.addressBuilding)
                FieldRow("PO box", card.addressPoBox)
                FieldRow("Telephone", card.addressPhone)
                FieldRow("Mobile", card.addressMobile)
                FieldRow("Email", card.addressEmail)
            }
        }

        Button(
            onClick = onContinue,
            modifier = Modifier.fillMaxWidth(),
        ) {
            Text("Continue to visit details", style = MaterialTheme.typography.labelLarge)
        }
    }
}
