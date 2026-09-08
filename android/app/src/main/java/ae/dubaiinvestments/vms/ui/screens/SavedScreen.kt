package ae.dubaiinvestments.vms.ui.screens

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
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.CheckCircle
import androidx.compose.material3.Button
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import java.time.OffsetDateTime
import java.time.ZoneId
import java.time.format.DateTimeFormatter

/** Step four. The visit is in the database, and the desk is told which record it is. */
@Composable
fun SavedScreen(state: UiState, onNext: () -> Unit) {
    val saved = state.saved

    Column(verticalArrangement = Arrangement.spacedBy(16.dp)) {

        SectionCard {
            Row(verticalAlignment = Alignment.CenterVertically) {
                Icon(
                    Icons.Default.CheckCircle,
                    contentDescription = null,
                    modifier = Modifier.size(34.dp),
                    tint = MaterialTheme.colorScheme.primary,
                )
                Spacer(Modifier.size(14.dp))
                Column(Modifier.weight(1f)) {
                    Text("Visit recorded", style = MaterialTheme.typography.headlineSmall)
                    Text(
                        state.visitorName,
                        style = MaterialTheme.typography.bodyLarge,
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                    )
                }
            }

            FieldRow("Reference", saved?.id?.let { "#$it" })
            FieldRow("Recorded at", saved?.recordedAtUtc?.let(::localTime))
            FieldRow("Person to visit", state.personToVisit)
            FieldRow(
                "Purpose",
                if (state.purpose == state.otherPurpose) {
                    listOfNotNull(state.purpose, state.purposeOther.trim().ifBlank { null }).joinToString(" - ")
                } else {
                    state.purpose
                },
            )
            FieldRow("Captured", captureLabel(saved?.captureMethod))
        }

        /* The server sends a warning when the read could not be proved genuine. It is
           shown rather than swallowed: the record is marked the same way in the report,
           and reception should know which kind of record they just made. */
        saved?.warning?.let { warning ->
            SectionCard("Worth knowing") {
                Text(
                    warning,
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                )
            }
        }

        Button(
            onClick = onNext,
            modifier = Modifier
                .fillMaxWidth()
                .height(56.dp),
        ) {
            Text("Next visitor", style = MaterialTheme.typography.labelLarge)
        }
    }
}

/** The same words the web report uses, so one record does not have two names. */
private fun captureLabel(method: String?) = when (method) {
    "CardReader" -> "Card"
    "CardReaderUnverified" -> "Card (unverified)"
    "Manual" -> "Manual"
    null -> null
    else -> method
}

/**
 * The server records UTC and says so. Reception thinks in Gulf Standard Time, so this is
 * the one place the two are reconciled.
 */
private fun localTime(iso: String): String =
    runCatching {
        /* OffsetDateTime, not Instant: .NET writes the offset as "+00:00" and Android's
           Instant.parse only accepts a trailing Z. */
        OffsetDateTime.parse(iso)
            .atZoneSameInstant(ZoneId.systemDefault())
            .format(DateTimeFormatter.ofPattern("d MMM yyyy, HH:mm"))
    }.getOrDefault(iso)
