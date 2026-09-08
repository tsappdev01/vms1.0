package ae.dubaiinvestments.vms.ui

import ae.dubaiinvestments.vms.BuildConfig
import ae.dubaiinvestments.vms.VmsApplication
import ae.dubaiinvestments.vms.api.ApiException
import ae.dubaiinvestments.vms.api.EntityDto
import ae.dubaiinvestments.vms.api.ManualIdentity
import ae.dubaiinvestments.vms.api.PersonDto
import ae.dubaiinvestments.vms.api.SaveVisitRequest
import ae.dubaiinvestments.vms.api.SavedVisitDto
import ae.dubaiinvestments.vms.api.VmsApi
import ae.dubaiinvestments.vms.api.VmsClient
import ae.dubaiinvestments.vms.auth.Auth
import ae.dubaiinvestments.vms.auth.SignInCancelled
import ae.dubaiinvestments.vms.auth.SignInRequired
import ae.dubaiinvestments.vms.card.CardRead
import ae.dubaiinvestments.vms.card.CardReadException
import ae.dubaiinvestments.vms.card.EmiratesIdReader
import ae.dubaiinvestments.vms.card.ReaderState
import android.app.Activity
import android.util.Log
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.FlowPreview
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.debounce
import kotlinx.coroutines.flow.distinctUntilChanged
import kotlinx.coroutines.flow.map
import kotlinx.coroutines.launch

/** Where reception is in the check-in. One screen each. */
enum class Step { InsertCard, VisitorInformation, VisitDetails, Saved }

/** What reception typed when the chip would not read. */
data class ManualDraft(
    val idNumber: String = "",
    val cardNumber: String = "",
    val fullNameEnglish: String = "",
    val nationalityEnglish: String = "",
    val dateOfBirth: String = "",
    val expiryDate: String = "",
    val mobile: String = "",
) {
    /** The server checks this too. Checking it here as well means the desk is told
        before a round trip, not after one. */
    val isUsable: Boolean
        get() = idNumber.any(Char::isDigit) && fullNameEnglish.isNotBlank()
}

data class UiState(
    val step: Step = Step.InsertCard,

    val signedInAs: String? = null,
    val signInRequired: Boolean = false,

    val reader: ReaderState? = null,

    /** Non-null while something is happening, and it says what. */
    val busy: String? = null,

    val card: CardRead? = null,
    val readRequestId: String? = null,
    val manual: ManualDraft? = null,

    val entities: List<EntityDto> = emptyList(),
    val purposes: List<String> = emptyList(),
    val otherPurpose: String = "Other",
    val referenceError: String? = null,

    val entityId: Int? = null,
    val hostQuery: String = "",
    val hostResults: List<PersonDto> = emptyList(),
    val hostSearching: Boolean = false,
    val searchAllEntities: Boolean = false,
    val host: PersonDto? = null,

    val purpose: String? = null,
    val purposeOther: String = "",

    val error: String? = null,
    val saved: SavedVisitDto? = null,
) {
    /** The name to show at the top of the visitor screens, from whichever source. */
    val visitorName: String
        get() = card?.fullNameEnglish?.takeIf { it.isNotBlank() }
            ?: manual?.fullNameEnglish?.trim()?.takeIf { it.isNotBlank() }
            ?: "Visitor"

    val visitorIdNumber: String?
        get() = card?.idNumber?.takeIf { it.isNotBlank() } ?: manual?.idNumber?.takeIf { it.isNotBlank() }

    val isManualEntry: Boolean get() = manual != null

    /** What the save button needs before it is worth pressing. */
    val canSave: Boolean
        get() = entityId != null &&
            personToVisit.isNotBlank() &&
            purpose != null &&
            (purpose != otherPurpose || purposeOther.isNotBlank())

    /** A picked host, or what was typed. The server records the text either way and the
        ID only when a directory entry was chosen. */
    val personToVisit: String
        get() = host?.displayName ?: hostQuery.trim()
}

class VisitorViewModel(
    private val api: VmsApi,
    private val auth: Auth,
    private val reader: EmiratesIdReader,
) : ViewModel() {

    /* Sign-in is a build-time switch, matching the server's Authentication:Enabled. Off,
       MSAL is never touched: asking it for the current account would initialise a client
       against a configuration file this build does not carry, and the failure would show
       up as a sign-in gate nobody can get past. */
    private val authEnabled = BuildConfig.AUTH_ENABLED

    private companion object {
        const val TAG = "VmsViewModel"

        /** How often the reader is asked whether a card is in it. Fast enough that
            inserting a card looks instant, slow enough not to hammer the native layer. */
        const val ReaderPollMillis = 1_200L
    }

    private val _state = MutableStateFlow(UiState())
    val state: StateFlow<UiState> = _state.asStateFlow()

    /* Declared before init, and it matters. viewModelScope dispatches on
       Dispatchers.Main.immediate, so a coroutine launched from init starts running
       straight away on the constructing thread - before any property declared below init
       has been assigned. watchHostQuery() touches this on its first line, so with the
       declaration further down the app crashed on launch with a null dereference on a
       val that cannot be null. */
    private val hostQueries = MutableStateFlow("")

    init {
        refreshAccount()
        loadReference()
        watchHostQuery()
    }

    // ---------------------------------------------------------------- sign-in

    private fun refreshAccount() = viewModelScope.launch {
        if (!authEnabled) return@launch

        val name = runCatching { auth.currentAccountName() }.getOrNull()
        _state.value = _state.value.copy(signedInAs = name, signInRequired = name == null)
    }

    /** A 401 only means "sign in" if signing in is something this build can do. */
    private fun needsSignIn(status: Int?) = authEnabled && status == 401

    fun signIn(activity: Activity) = viewModelScope.launch {
        _state.value = _state.value.copy(busy = "Signing in…", error = null)
        try {
            auth.token(activity)
            val name = auth.currentAccountName()
            _state.value = _state.value.copy(busy = null, signedInAs = name, signInRequired = name == null)
            loadReference()
        } catch (e: SignInCancelled) {
            _state.value = _state.value.copy(busy = null)
        } catch (e: Exception) {
            Log.w(TAG, "Sign-in failed", e)
            _state.value = _state.value.copy(busy = null, error = "Sign-in failed. ${e.message}")
        }
    }

    fun signOut() = viewModelScope.launch {
        if (!authEnabled) return@launch

        auth.signOut()
        /* Everything on the screen belongs to the officer who is leaving, including a
           visitor's photograph. */
        _state.value = UiState(signInRequired = true)
    }

    // ---------------------------------------------------------------- reference data

    fun loadReference() = viewModelScope.launch {
        try {
            val reference = VmsClient.call { api.reference() }
            _state.value = _state.value.copy(
                entities = reference.entities,
                purposes = reference.purposes,
                otherPurpose = reference.otherPurpose,
                referenceError = null,
                signInRequired = false,
                /* One entity is not a choice. Pre-selecting it saves a tap at every
                   check-in and cannot be wrong. */
                entityId = _state.value.entityId ?: reference.entities.singleOrNull()?.id,
            )
        } catch (e: ApiException) {
            _state.value = _state.value.copy(
                referenceError = e.message,
                signInRequired = needsSignIn(e.status),
            )
        }
    }

    // ---------------------------------------------------------------- the reader

    /**
     * Keeps the Insert Card screen's reader panel current. Called from the screen so it
     * stops when the screen goes away - a poll running behind a finished check-in would
     * hold the reader open and keep the tablet awake.
     */
    suspend fun watchReader() {
        while (true) {
            val readerState = runCatching { reader.state() }.getOrNull()
            if (readerState != null) _state.value = _state.value.copy(reader = readerState)
            delay(ReaderPollMillis)
        }
    }

    fun readCard() = viewModelScope.launch {
        _state.value = _state.value.copy(busy = "Starting…", error = null)

        try {
            /* The server issues the request ID. It is what stops a response captured off
               one tablet being posted from another, so it cannot be chosen here. */
            val ticket = VmsClient.call { api.beginRead() }

            val card = reader.read(ticket.requestId) { phase ->
                _state.value = _state.value.copy(busy = phase)
            }

            _state.value = _state.value.copy(
                busy = null,
                card = card,
                readRequestId = ticket.requestId,
                manual = null,
                step = Step.VisitorInformation,
            )
        } catch (e: SignInRequired) {
            _state.value = _state.value.copy(busy = null, signInRequired = authEnabled)
        } catch (e: ApiException) {
            _state.value = _state.value.copy(
                busy = null,
                error = e.message,
                signInRequired = needsSignIn(e.status),
            )
        } catch (e: CardReadException) {
            _state.value = _state.value.copy(busy = null, error = e.message)
        }
    }

    // ---------------------------------------------------------------- manual entry

    fun useManualEntry(draft: ManualDraft) {
        _state.value = _state.value.copy(
            card = null,
            readRequestId = null,
            manual = draft,
            error = null,
            step = Step.VisitorInformation,
        )
    }

    // ---------------------------------------------------------------- the visit

    fun setEntity(id: Int) {
        /* The host list is filtered by entity unless the desk asks otherwise, so a
           different entity invalidates a host chosen under the old one. */
        _state.value = _state.value.copy(entityId = id, host = null)
        hostQueries.value = _state.value.hostQuery
    }

    fun setHostQuery(query: String) {
        _state.value = _state.value.copy(hostQuery = query, host = null)
        hostQueries.value = query
    }

    fun selectHost(person: PersonDto) {
        _state.value = _state.value.copy(host = person, hostQuery = person.displayName, hostResults = emptyList())
    }

    fun setSearchAllEntities(all: Boolean) {
        _state.value = _state.value.copy(searchAllEntities = all)
        hostQueries.value = _state.value.hostQuery
    }

    fun setPurpose(purpose: String) {
        _state.value = _state.value.copy(purpose = purpose)
    }

    fun setPurposeOther(text: String) {
        _state.value = _state.value.copy(purposeOther = text)
    }

    @OptIn(FlowPreview::class)
    private fun watchHostQuery() = viewModelScope.launch {
        /* Debounced, because a search per keystroke is a query per keystroke against a
           725-row directory over office wifi. */
        hostQueries
            .map(String::trim)
            .debounce(300)
            .distinctUntilChanged()
            .collect { term ->
                if (term.length < 2) {
                    _state.value = _state.value.copy(hostResults = emptyList(), hostSearching = false)
                    return@collect
                }

                if (_state.value.host != null) return@collect

                _state.value = _state.value.copy(hostSearching = true)
                try {
                    val current = _state.value
                    val people = VmsClient.call {
                        api.people(term, current.entityId, current.searchAllEntities)
                    }
                    _state.value = _state.value.copy(hostResults = people, hostSearching = false)
                } catch (e: ApiException) {
                    Log.w(TAG, "Host search failed: ${e.message}")
                    _state.value = _state.value.copy(hostSearching = false)
                }
            }
    }

    fun toVisitDetails() {
        _state.value = _state.value.copy(step = Step.VisitDetails, error = null)
    }

    fun backToVisitorInformation() {
        _state.value = _state.value.copy(step = Step.VisitorInformation, error = null)
    }

    fun save() = viewModelScope.launch {
        val current = _state.value
        if (!current.canSave) return@launch

        _state.value = current.copy(busy = "Saving…", error = null)

        val request = SaveVisitRequest(
            requestId = current.readRequestId,
            readResponseXml = current.card?.responseXml,
            manual = current.manual?.let {
                ManualIdentity(
                    idNumber = it.idNumber.trim(),
                    cardNumber = it.cardNumber.trim().ifBlank { null },
                    fullNameEnglish = it.fullNameEnglish.trim(),
                    nationalityEnglish = it.nationalityEnglish.trim().ifBlank { null },
                    dateOfBirth = it.dateOfBirth.trim().ifBlank { null },
                    expiryDate = it.expiryDate.trim().ifBlank { null },
                    addressMobile = it.mobile.trim().ifBlank { null },
                )
            },
            entityId = current.entityId!!,
            personToVisit = current.personToVisit,
            personToVisitId = current.host?.id,
            purpose = current.purpose!!,
            purposeOther = current.purposeOther.trim().ifBlank { null },
        )

        try {
            val saved = VmsClient.call { api.saveVisit(request) }
            _state.value = _state.value.copy(busy = null, saved = saved, step = Step.Saved)
        } catch (e: ApiException) {
            /* A rejected read is the one failure with a recovery: the request ID is spent
               after five minutes, and the answer is to read the card again. The visit
               details stay exactly as they are, so that is one tap and not a re-typed
               form. */
            val readRejected = e.status == 400 && current.card != null

            _state.value = _state.value.copy(
                busy = null,
                error = e.message,
                signInRequired = needsSignIn(e.status),
                step = if (readRejected) Step.InsertCard else _state.value.step,
                card = if (readRejected) null else _state.value.card,
                readRequestId = if (readRejected) null else _state.value.readRequestId,
            )
        }
    }

    /** A finished check-in, cleared for the next visitor. Details included: the next
        visitor is not necessarily here to see the same person. */
    fun startOver() {
        val current = _state.value
        _state.value = UiState(
            signedInAs = current.signedInAs,
            entities = current.entities,
            purposes = current.purposes,
            otherPurpose = current.otherPurpose,
            entityId = current.entityId,
            searchAllEntities = current.searchAllEntities,
            reader = current.reader,
        )
        hostQueries.value = ""
    }

    fun dismissError() {
        _state.value = _state.value.copy(error = null)
    }

    /** Built by hand for the same reason the rest is: three dependencies, all of them
        already living on the Application. */
    class Factory(private val application: VmsApplication) : ViewModelProvider.Factory {
        @Suppress("UNCHECKED_CAST")
        override fun <T : ViewModel> create(modelClass: Class<T>): T =
            VisitorViewModel(application.api, application.auth, application.reader) as T
    }
}
