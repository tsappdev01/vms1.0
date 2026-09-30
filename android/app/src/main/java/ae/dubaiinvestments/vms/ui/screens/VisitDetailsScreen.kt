package ae.dubaiinvestments.vms.ui.screens

import ae.dubaiinvestments.vms.api.PersonDto
import ae.dubaiinvestments.vms.ui.FieldRules
import ae.dubaiinvestments.vms.ui.UiState
import ae.dubaiinvestments.vms.ui.parts.SectionCard
import androidx.compose.foundation.clickable
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.ExperimentalLayoutApi
import androidx.compose.foundation.layout.FlowRow
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ArrowBack
import androidx.compose.material.icons.filled.Check
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.ExposedDropdownMenuBox
import androidx.compose.material3.ExposedDropdownMenuDefaults
import androidx.compose.material3.FilterChip
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
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
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp

/**
 * Step three. Who they are here to see, and why.
 *
 * The host is searched on the server rather than picked from a list held here. The
 * directory is 725 people with titles, emails and employers; that is not a file to leave
 * on a tablet at a reception desk, and a search is two round trips against a database
 * that already has an index for it.
 */
@OptIn(ExperimentalMaterial3Api::class, ExperimentalLayoutApi::class)
@Composable
fun VisitDetailsScreen(
    state: UiState,
    onRetryReference: () -> Unit,
    onEntity: (Int) -> Unit,
    onHostQuery: (String) -> Unit,
    onSelectHost: (PersonDto) -> Unit,
    onSearchAllEntities: (Boolean) -> Unit,
    onContactMobile: (String) -> Unit,
    onPurpose: (String) -> Unit,
    onPurposeOther: (String) -> Unit,
    onBack: () -> Unit,
    onSave: () -> Unit,
) {
    Column(verticalArrangement = Arrangement.spacedBy(16.dp)) {

        SectionCard("Visiting") {
            var entityMenuOpen by remember { mutableStateOf(false) }
            val entityName = state.entities.firstOrNull { it.id == state.entityId }?.name.orEmpty()

            ExposedDropdownMenuBox(
                expanded = entityMenuOpen,
                onExpandedChange = { entityMenuOpen = it },
            ) {
                OutlinedTextField(
                    value = entityName,
                    onValueChange = {},
                    readOnly = true,
                    label = { Text("Entity") },
                    trailingIcon = { ExposedDropdownMenuDefaults.TrailingIcon(entityMenuOpen) },
                    modifier = Modifier
                        .fillMaxWidth()
                        .menuAnchor(),
                )
                ExposedDropdownMenu(
                    expanded = entityMenuOpen,
                    onDismissRequest = { entityMenuOpen = false },
                ) {
                    state.entities.forEach { entity ->
                        DropdownMenuItem(
                            text = { Text(entity.name) },
                            onClick = {
                                onEntity(entity.id)
                                entityMenuOpen = false
                            },
                        )
                    }
                }
            }

            if (state.entities.isEmpty()) {
                /* A way out, because there was not one. A desk that lost the race against a
                   cold server used to be stuck with an empty form until the app was
                   force-closed, which is not something to ask of somebody with a visitor in
                   front of them. */
                if (!state.referenceLoading && state.referenceError != null) {
                    TextButton(onClick = onRetryReference) { Text("Try again") }
                }

                Text(
                    when {
                        state.referenceLoading -> "Waiting for the server\u2026"
                        else -> state.referenceError ?: "The entity list has not loaded yet."
                    },
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.error,
                )
            }

            val chosenHost = state.host
            val hostHint = if (chosenHost == null) {
                "Type at least two letters, or write the name if they are not listed."
            } else {
                listOfNotNull(chosenHost.title, chosenHost.companyName)
                    .joinToString(" · ")
                    .ifBlank { "From the directory" }
            }

            OutlinedTextField(
                value = state.hostQuery,
                onValueChange = onHostQuery,
                label = { Text("Person to visit") },
                supportingText = { Text(hostHint) },
                trailingIcon = {
                    when {
                        state.hostSearching -> CircularProgressIndicator(
                            modifier = Modifier.size(18.dp),
                            strokeWidth = 2.dp,
                        )
                        chosenHost != null -> Icon(
                            Icons.Default.Check,
                            contentDescription = "Chosen from the directory",
                            tint = MaterialTheme.colorScheme.primary,
                        )
                    }
                },
                modifier = Modifier.fillMaxWidth(),
            )

            if (state.hostResults.isNotEmpty()) {
                Column {
                    state.hostResults.forEach { person ->
                        HorizontalDivider()
                        Column(
                            Modifier
                                .fillMaxWidth()
                                .clickable { onSelectHost(person) }
                                .padding(vertical = 10.dp),
                        ) {
                            Text(
                                person.displayName,
                                style = MaterialTheme.typography.bodyLarge,
                                fontWeight = FontWeight.Medium,
                            )
                            val subtitle = listOfNotNull(person.title, person.companyName).joinToString(" · ")
                            if (subtitle.isNotBlank()) {
                                Text(
                                    subtitle,
                                    style = MaterialTheme.typography.bodyMedium,
                                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                                )
                            }
                        }
                    }
                }
            }

            Row(verticalAlignment = Alignment.CenterVertically) {
                Switch(checked = state.searchAllEntities, onCheckedChange = onSearchAllEntities)
                Spacer(Modifier.size(12.dp))
                Column(Modifier.weight(1f)) {
                    Text("Search every entity", style = MaterialTheme.typography.bodyLarge)
                    Text(
                        "Off, the search stays inside the entity above.",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant,
                    )
                }
            }
        }

        SectionCard("Contact") {
            /* Typed, not read. The card's own mobile is parsed out of the signed XML on
               the server; on every card tested at DIP that field came back empty, so this
               is the number that will actually reach the visitor today. */
            /* Required, and it says so. It was marked optional here while the desk browser
               required it, which is the kind of disagreement that only shows up as a visitor
               nobody can ring. */
            val mobileConcern = FieldRules.mobileConcern(state.contactMobile)

            OutlinedTextField(
                value = state.contactMobile,
                onValueChange = onContactMobile,
                label = { Text("Mobile number") },
                isError = mobileConcern != null,
                supportingText = {
                    Text(mobileConcern ?: "How to reach the visitor. Required.")
                },
                singleLine = true,
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Phone),
                modifier = Modifier.fillMaxWidth(),
            )
        }

        SectionCard("Purpose") {
            FlowRow(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                (state.purposes + state.otherPurpose).forEach { purpose ->
                    FilterChip(
                        selected = state.purpose == purpose,
                        onClick = { onPurpose(purpose) },
                        label = { Text(purpose) },
                    )
                }
            }

            if (state.purpose == state.otherPurpose) {
                OutlinedTextField(
                    value = state.purposeOther,
                    onValueChange = onPurposeOther,
                    label = { Text("Details") },
                    singleLine = false,
                    minLines = 2,
                    modifier = Modifier.fillMaxWidth(),
                )
            }
        }

        /*  Named, not greyed.
         *
         *  A disabled button with nothing beside it is the single most common reason a desk
         *  rings somebody. The desk browser has said this for a while; the tablet only
         *  greyed the button out. */
        if (state.missingRequired.isNotEmpty()) {
            Text(
                "Still needed: ${state.missingRequired.joinToString(", ")}.",
                style = MaterialTheme.typography.bodyMedium,
                color = MaterialTheme.colorScheme.error,
            )
        }

        /* And what the save will go ahead with anyway. Refusing here would send reception
           for a pen and paper; saying it lets them ask the visitor one more question. */
        if (state.concerns.isNotEmpty()) {
            Text(
                "Worth checking: ${state.concerns.joinToString(" ")}",
                style = MaterialTheme.typography.bodyMedium,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
            )
        }

        Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
            TextButton(onClick = onBack, modifier = Modifier.height(56.dp)) {
                Icon(Icons.Default.ArrowBack, contentDescription = null, modifier = Modifier.size(18.dp))
                Spacer(Modifier.size(8.dp))
                Text("Back")
            }

            Button(
                onClick = onSave,
                enabled = state.canSave && state.busy == null,
                modifier = Modifier
                    .weight(1f)
                    .height(56.dp),
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
                    Text("Record the visit", style = MaterialTheme.typography.labelLarge)
                }
            }
        }
    }
}
