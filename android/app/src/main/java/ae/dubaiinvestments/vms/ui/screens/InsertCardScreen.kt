package ae.dubaiinvestments.vms.ui.screens

import ae.dubaiinvestments.vms.ui.UiState
import ae.dubaiinvestments.vms.ui.parts.SectionCard
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.background
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.CheckCircle
import androidx.compose.material.icons.filled.CreditCard
import androidx.compose.material.icons.filled.Edit
import androidx.compose.material.icons.filled.Usb
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp

/**
 * Step one. One instruction and one button.
 *
 * The reader panel below is the part reception actually uses: when a read fails, the
 * question is always whether the reader is there and whether the card is in it, and the
 * answer is on the screen before anybody rings IT.
 */
@Composable
fun InsertCardScreen(
    state: UiState,
    onReadCard: () -> Unit,
    onManualEntry: () -> Unit,
) {
    Column(verticalArrangement = Arrangement.spacedBy(16.dp)) {

        SectionCard {
            Row(verticalAlignment = Alignment.CenterVertically) {
                Icon(
                    Icons.Default.CreditCard,
                    contentDescription = null,
                    modifier = Modifier.size(34.dp),
                    tint = MaterialTheme.colorScheme.primary,
                )
                Spacer(Modifier.size(14.dp))
                Column(Modifier.weight(1f)) {
                    Text("Insert the Emirates ID", style = MaterialTheme.typography.headlineSmall)
                    Text(
                        "Chip first, face up, into the reader on the desk.",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                    )
                }
            }

            Spacer(Modifier.height(4.dp))

            Button(
                onClick = onReadCard,
                enabled = state.busy == null,
                modifier = Modifier.fillMaxWidth().height(56.dp),
            ) {
                if (state.busy != null) {
                    CircularProgressIndicator(
                        modifier = Modifier.size(20.dp),
                        strokeWidth = 2.dp,
                        color = MaterialTheme.colorScheme.onPrimary,
                    )
                    Spacer(Modifier.size(12.dp))
                    Text(state.busy)
                } else {
                    Text("Read card", style = MaterialTheme.typography.labelLarge)
                }
            }

            TextButton(
                onClick = onManualEntry,
                enabled = state.busy == null,
                modifier = Modifier.fillMaxWidth(),
            ) {
                Icon(Icons.Default.Edit, contentDescription = null, modifier = Modifier.size(18.dp))
                Spacer(Modifier.size(8.dp))
                Text("The chip will not read - enter the details by hand")
            }
        }

        SectionCard("Reader") {
            val reader = state.reader

            Row(verticalAlignment = Alignment.CenterVertically) {
                val ready = reader?.cardPresent == true
                val detected = reader?.readerName != null

                Icon(
                    if (ready) Icons.Default.CheckCircle else Icons.Default.Usb,
                    contentDescription = null,
                    modifier = Modifier.size(22.dp),
                    tint = when {
                        ready -> MaterialTheme.colorScheme.primary
                        detected -> MaterialTheme.colorScheme.onSurfaceVariant
                        else -> MaterialTheme.colorScheme.error
                    },
                )
                Spacer(Modifier.size(10.dp))
                Column(Modifier.weight(1f)) {
                    Text(
                        reader?.readerName ?: "No reader connected",
                        style = MaterialTheme.typography.bodyLarge,
                        fontWeight = FontWeight.Medium,
                    )
                    Text(
                        reader?.detail ?: "Looking for a reader…",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                    )
                }
            }

            if (reader?.readerName == null) {
                Text(
                    "Plug the ACR39U into the tablet's USB-C port, through the OTG adapter " +
                        "if the reader has a full-size plug.",
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    modifier = Modifier
                        .fillMaxWidth()
                        .background(MaterialTheme.colorScheme.surfaceVariant, RoundedCornerShape(10.dp))
                        .padding(12.dp),
                )
            }

            val toolkitLine = listOfNotNull(
                reader?.toolkitVersion?.let { "Toolkit $it" },
                reader?.licenceExpiry?.let { "licence to $it" },
            ).joinToString(" · ")

            if (toolkitLine.isNotBlank()) {
                Text(
                    toolkitLine,
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                )
            }
        }
    }
}
