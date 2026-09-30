package ae.dubaiinvestments.vms.ui

import ae.dubaiinvestments.vms.VmsApplication
import ae.dubaiinvestments.vms.api.ApiException
import ae.dubaiinvestments.vms.api.ApiProvider
import ae.dubaiinvestments.vms.api.EntityDto
import ae.dubaiinvestments.vms.api.ManualIdentity
import ae.dubaiinvestments.vms.api.MrzRequest
import ae.dubaiinvestments.vms.api.PersonDto
import ae.dubaiinvestments.vms.api.SaveVisitRequest
import ae.dubaiinvestments.vms.api.SavedVisitDto
import ae.dubaiinvestments.vms.api.VmsClient
import ae.dubaiinvestments.vms.card.CardRead
import ae.dubaiinvestments.vms.card.CardReadException
import ae.dubaiinvestments.vms.card.EmiratesIdReader
import ae.dubaiinvestments.vms.card.ReaderState
import ae.dubaiinvestments.vms.settings.DeskPin
import ae.dubaiinvestments.vms.settings.PinVerdict
import ae.dubaiinvestments.vms.settings.ServerSettings
import ae.dubaiinvestments.vms.settings.Settings
import android.util.Log
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.FlowPreview
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.debounce
import kotlinx.coroutines.flow.distinctUntilChanged
import kotlinx.coroutines.flow.map
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext

/** Where reception is in the check-in. One screen each. */
enum class Step { InsertCard, MrzScan, VisitorInformation, VisitDetails, Saved }

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
    /**
     * What the dialog needs before "Use these details" is worth pressing.
     *
     * The same three the server requires, checked here so the desk is told before a round
     * trip rather than after one. The ID number used to be "contains a digit", which meant
     * the tablet would happily send "7" and let the save come back refused - or, before the
     * server checked either, record it.
     */
    val isUsable: Boolean
        get() = FieldRules.typedIdNumberProblem(idNumber) == null &&
            fullNameEnglish.isNotBlank() &&
            expiryDate.isNotBlank()
}

data class UiState(
    val step: Step = Step.InsertCard,

    val reader: ReaderState? = null,

    /** Non-null while something is happening, and it says what. */
    val busy: String? = null,

    val card: CardRead? = null,
    val readRequestId: String? = null,
    val manual: ManualDraft? = null,

    /** The text a camera read off a card, kept so the save can send it. The server parses it
        again there and decides the provenance from what it parsed, which is why the fields
        below are a preview and not the record. */
    val mrzText: String? = null,
    /** False when only the printed ID number could be salvaged - the officer then has the
        number and has to type or scan the rest. */
    val mrzComplete: Boolean = false,

    val entities: List<EntityDto> = emptyList(),
    val purposes: List<String> = emptyList(),
    val otherPurpose: String = "Other",
    val referenceError: String? = null,
    /** True while the lists are being asked for again after a failure. */
    val referenceLoading: Boolean = false,

    val entityId: Int? = null,
    val hostQuery: String = "",
    val hostResults: List<PersonDto> = emptyList(),
    val hostSearching: Boolean = false,
    /** On by default, matching the desk. The export's company names do not all map onto the
        entity list, so a host with no mapping is invisible until this is on - and nothing on
        the screen says that is why. */
    val searchAllEntities: Boolean = true,
    val host: PersonDto? = null,

    val purpose: String? = null,
    val purposeOther: String = "",

    /** The number the visitor gives at the desk. Typed, always: the card's own mobile is
        parsed out of the signed XML on the server, so there is nothing here to prefill
        from - and on every card tested at DIP that field came back empty anyway. */
    val contactMobile: String = "",

    /** Where this tablet sends its visits. Shown in the top bar and edited under the gear. */
    val server: ServerSettings,
    val serverIsDefault: Boolean = true,
    val settingsOpen: Boolean = false,
    val settingsBusy: String? = null,
    /** The result of Test connection, good or bad, in the words to put on the screen. */
    val settingsMessage: String? = null,
    val settingsFailed: Boolean = false,

    /** A PIN has been set on this tablet, so the settings screen asks for it. */
    val pinIsSet: Boolean = false,
    /** The right PIN has been typed, and stays typed only while settings is open. */
    val pinUnlocked: Boolean = false,
    /** What to say under the PIN field - wrong, locked out, or changed. */
    val pinMessage: String? = null,
    val pinFailed: Boolean = false,
    /** Seconds left of a lockout, ticked down so the screen can count with it. */
    val pinLockedSeconds: Int = 0,

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

    /** Settings is open but showing the PIN pad rather than the key. */
    val settingsLocked: Boolean get() = settingsOpen && pinIsSet && !pinUnlocked

    /**
     * What the visit still needs, named rather than counted.
     *
     * One list, and [canSave] is derived from it, so the button and the sentence explaining
     * the button cannot drift apart - which is how the mobile number came to be required on
     * the desk browser and optional here.
     */
    val missingRequired: List<String>
        get() = buildList {
            if (entityId == null) add("entity")
            if (personToVisit.isBlank()) add("person to visit")
            if (purpose == null) add("purpose")
            if (purpose == otherPurpose && purposeOther.isBlank()) add("details for Other")
            if (contactMobile.isBlank()) add("mobile number")
        }

    /** What the save button needs before it is worth pressing. */
    val canSave: Boolean get() = missingRequired.isEmpty()

    /**
     * What is worth saying about this visit without refusing it.
     *
     * Read off whichever source produced the fields, so a chip read that came back with an
     * expired date is flagged exactly as a typed one is.
     */
    val concerns: List<String>
        get() = listOfNotNull(
            FieldRules.expiryConcern(manual?.expiryDate ?: card?.expiryDate),
            FieldRules.mobileConcern(contactMobile),
        )

    /** A picked host, or what was typed. The server records the text either way and the
        ID only when a directory entry was chosen. */
    val personToVisit: String
        get() = host?.displayName ?: hostQuery.trim()
}

/**
 * The check-in, and the one setting a desk can change.
 *
 * Nobody signs in. The tablet is the credential - it sits on a counter, it is handed to
 * nobody, and it identifies itself to the server with an API key rather than a person.
 * That is why a visit is recorded against the tablet and not against an officer, and it is
 * the reason there is no sign-in screen to get past when a visitor is already standing
 * there.
 */
class VisitorViewModel(
    private val apis: ApiProvider,
    private val settings: Settings,
    private val pin: DeskPin,
    private val reader: EmiratesIdReader,
) : ViewModel() {

    private companion object {
        const val TAG = "VmsViewModel"

        /** Four tries, roughly fifteen seconds in all - about what a cold start costs. */
        const val Attempts = 4

        /** How often the reader is asked whether a card is in it. Fast enough that
            inserting a card looks instant, slow enough not to hammer the native layer. */
        const val ReaderPollMillis = 1_200L
    }

    private val _state = MutableStateFlow(
        UiState(server = settings.value, serverIsDefault = settings.isDefault),
    )
    val state: StateFlow<UiState> = _state.asStateFlow()

    /* Declared before init, and it matters. viewModelScope dispatches on
       Dispatchers.Main.immediate, so a coroutine launched from init starts running
       straight away on the constructing thread - before any property declared below init
       has been assigned. watchHostQuery() touches this on its first line, so with the
       declaration further down the app crashed on launch with a null dereference on a
       val that cannot be null. */
    private val hostQueries = MutableStateFlow("")

    /** Ticks a PIN lockout down on the screen. Declared here with the other long-lived
        job, above init, for the reason stated about hostQueries. */
    private var countdown: Job? = null

    /** The PIN check in flight, so a second tap cannot spend a second attempt on the same
        guess while the first is still hashing. */
    private var pinJob: Job? = null

    init {
        loadReference()
        watchHostQuery()
    }

    // ---------------------------------------------------------------- settings

    fun openSettings() {
        val locked = pin.isSet
        _state.value = _state.value.copy(
            settingsOpen = true,
            settingsMessage = null,
            settingsFailed = false,
            pinIsSet = locked,
            /* No PIN set means no gate. A tablet being set up for the first time cannot be
               locked out of the screen where the PIN is set, and a deployment that has not
               got round to it keeps working exactly as it did. */
            pinUnlocked = !locked,
            pinMessage = null,
            pinFailed = false,
        )
        watchLockout()
    }

    fun closeSettings() {
        countdown?.cancel()
        /* And the check in flight, or its result lands after the screen was re-locked and
           unlocks the next opening without a PIN having been typed for it. */
        pinJob?.cancel()
        _state.value = _state.value.copy(
            settingsOpen = false,
            settingsBusy = null,
            /* Re-locked on the way out, always. The screen reception leaves open behind them
               is the one somebody else walks up to, and a PIN that is asked for once a day
               is not a PIN. */
            pinUnlocked = false,
            pinMessage = null,
            pinFailed = false,
        )
    }

    /**
     * Tries an address before it is saved.
     *
     * Worth its own button: a mistyped host otherwise shows up as a failure on the next
     * check-in, in front of a visitor, with nothing connecting it to what was typed here.
     */
    fun testServer(baseUrl: String, apiKey: String) = viewModelScope.launch {
        val normalised = Settings.normalise(baseUrl)
        if (normalised == null) {
            _state.value = _state.value.copy(
                settingsMessage = "That is not an address the tablet can use.",
                settingsFailed = true,
            )
            return@launch
        }

        _state.value = _state.value.copy(
            settingsBusy = "Trying…",
            settingsMessage = null,
            settingsFailed = false,
        )

        /* A client of its own, against the typed address rather than the saved one, so
           testing cannot leave the tablet pointed somewhere it was not meant to go. */
        val candidate = VmsClient.create(ServerSettings(normalised, apiKey.trim()))

        try {
            val reference = VmsClient.call { candidate.reference() }
            _state.value = _state.value.copy(
                settingsBusy = null,
                settingsMessage = "Reached the server. It offers ${reference.entities.size} " +
                    "entities and ${reference.purposes.size} purposes.",
                settingsFailed = false,
            )
        } catch (e: ApiException) {
            _state.value = _state.value.copy(
                settingsBusy = null,
                settingsMessage = e.message,
                settingsFailed = true,
            )
        }
    }

    fun saveServer(baseUrl: String, apiKey: String) {
        val failure = settings.save(baseUrl, apiKey)
        if (failure != null) {
            _state.value = _state.value.copy(settingsMessage = failure, settingsFailed = true)
            return
        }

        applySettings()
    }

    fun resetServer() {
        settings.resetToDefault()
        applySettings()
    }

    /** A new address means the reference data on the screen belongs to the old one. */
    private fun applySettings() {
        countdown?.cancel()
        pinJob?.cancel()

        _state.value = _state.value.copy(
            server = settings.value,
            serverIsDefault = settings.isDefault,
            settingsOpen = false,
            settingsBusy = null,
            settingsMessage = null,
            settingsFailed = false,
            /* Saving closes the screen, and closing it re-locks - the same rule as the back
               arrow, stated here so the two ways out cannot drift apart. */
            pinUnlocked = false,
            pinMessage = null,
            pinFailed = false,
            entities = emptyList(),
            purposes = emptyList(),
            entityId = null,
            host = null,
            hostResults = emptyList(),
            referenceError = null,
        )
        loadReference()
    }

    // ---------------------------------------------------------------- the PIN

    /** A field that refuses with no end in sight looks broken; one that says "try again in
        24 seconds" is a tablet doing its job. */
    private fun watchLockout() {
        countdown?.cancel()
        countdown = viewModelScope.launch {
            while (true) {
                val remaining = pin.lockedForMillis()
                val seconds = ((remaining + 999) / 1000).toInt()
                if (_state.value.pinLockedSeconds != seconds) {
                    _state.value = _state.value.copy(pinLockedSeconds = seconds)
                }
                if (remaining <= 0) return@launch
                delay(500)
            }
        }
    }

    /**
     * The typed PIN, checked.
     *
     * Off the main thread, and one at a time. PBKDF2 is deliberately not instant, so running
     * it where the frames are drawn is a stutter at best; and two taps on the button while
     * the first is still hashing would spend two of the four attempts the lockout allows on
     * one guess.
     */
    fun submitPin(typed: String) {
        if (pinJob?.isActive == true) return

        pinJob = viewModelScope.launch {
            when (val verdict = withContext(Dispatchers.Default) { pin.verify(typed) }) {
                is PinVerdict.Correct ->
                    _state.value = _state.value.copy(
                        pinUnlocked = true,
                        pinMessage = null,
                        pinFailed = false,
                        pinLockedSeconds = 0,
                    )

                is PinVerdict.Wrong ->
                    refusePin(
                        if (verdict.lockedForMillis > 0) {
                            "That is not the PIN. Too many tries - wait " +
                                "${DeskPin.waitInWords(verdict.lockedForMillis)}."
                        } else {
                            "That is not the PIN."
                        },
                    )

                is PinVerdict.LockedOut ->
                    refusePin(
                        "Too many tries. Wait ${DeskPin.waitInWords(verdict.remainingMillis)}.",
                    )
            }
            watchLockout()
        }
    }

    /**
     * Sets a PIN, or replaces one.
     *
     * The current PIN is asked for even though the screen is already unlocked, because the
     * case this guards is not somebody who got past the gate - it is a settings screen left
     * open on the counter, which is the way a tablet is actually reached.
     *
     * The new PIN is checked before the current one on purpose. Verifying costs an attempt
     * against the lockout, and mistyping the confirmation is a fumble rather than a guess -
     * it should not walk anyone towards a fifteen-minute wait.
     */
    fun setPin(current: String, new: String, confirm: String) {
        DeskPin.validate(new)?.let { return refusePin(it) }
        if (new != confirm) return refusePin("The two new PINs are not the same.")
        if (pinJob?.isActive == true) return

        pinJob = viewModelScope.launch {
            if (pin.isSet) {
                val problem = withContext(Dispatchers.Default) { checkCurrent(current) }
                if (problem != null) {
                    refusePin(problem)
                    watchLockout()
                    return@launch
                }
            }

            val failure = withContext(Dispatchers.Default) { pin.set(new) }
            _state.value = _state.value.copy(
                pinIsSet = pin.isSet,
                pinFailed = failure != null,
                pinMessage = failure
                    ?: "PIN set. Settings will ask for it from now on - write it down " +
                    "somewhere that is not this tablet, because there is no way to recover it.",
                pinLockedSeconds = 0,
            )
        }
    }

    /** Takes the PIN off, on the current one. */
    fun removePin(current: String) {
        if (pinJob?.isActive == true) return

        pinJob = viewModelScope.launch {
            val problem = withContext(Dispatchers.Default) { checkCurrent(current) }
            if (problem != null) {
                refusePin(problem)
                watchLockout()
                return@launch
            }

            pin.clear()
            _state.value = _state.value.copy(
                pinIsSet = false,
                pinUnlocked = true,
                /* Shown in the error colour although nothing failed. It is the one change on
                   this screen that makes the tablet less safe than it was, and it should not
                   slip past in grey as though it were a confirmation. */
                pinFailed = true,
                pinMessage = "PIN removed. Anyone holding this tablet can now read the API key.",
                pinLockedSeconds = 0,
            )
        }
    }

    /** null when it was right, or what to put on the screen. Hashes - call it off the main
        thread. */
    private fun checkCurrent(current: String): String? = when (val verdict = pin.verify(current)) {
        is PinVerdict.Correct -> null
        is PinVerdict.LockedOut ->
            "Too many tries. Wait ${DeskPin.waitInWords(verdict.remainingMillis)}."
        is PinVerdict.Wrong -> if (verdict.lockedForMillis > 0) {
            "The current PIN is wrong. Too many tries - wait " +
                "${DeskPin.waitInWords(verdict.lockedForMillis)}."
        } else {
            "The current PIN is wrong."
        }
    }

    private fun refusePin(message: String) {
        _state.value = _state.value.copy(pinFailed = true, pinMessage = message)
    }

    // ---------------------------------------------------------------- reference data

    /**
     * The entity and purpose lists, with the patience their timing needs.
     *
     * This runs the moment the app opens, which is the worst moment to ask: an App Service
     * that has scaled to zero takes the better part of a minute to answer its first request,
     * and a reception desk opens this app exactly when nobody has used it for hours. One
     * attempt with a thirty-second timeout loses that race, and losing it used to be
     * permanent - the lists stayed empty until somebody force-closed the app, and the screen
     * showed an entity error and a purpose list with nothing in it but "Other", which looks
     * like two faults and is one.
     *
     * So it waits and asks again. Four attempts over about fifteen seconds covers a cold
     * start; beyond that the server is genuinely down and saying so is the right answer.
     */
    fun loadReference() = viewModelScope.launch {
        var delayMs = 1_000L

        repeat(Attempts) { attempt ->
            try {
                val reference = VmsClient.call { apis.current().reference() }
                _state.value = _state.value.copy(
                    entities = reference.entities,
                    purposes = reference.purposes,
                    otherPurpose = reference.otherPurpose,
                    referenceError = null,
                    referenceLoading = false,
                    /* One entity is not a choice. Pre-selecting it saves a tap at every
                       check-in and cannot be wrong. */
                    entityId = _state.value.entityId ?: reference.entities.singleOrNull()?.id,
                )
                return@launch
            } catch (e: ApiException) {
                val last = attempt == Attempts - 1

                _state.value = _state.value.copy(
                    referenceError = if (last) e.message else null,
                    referenceLoading = !last,
                )

                if (last) return@launch

                delay(delayMs)
                delayMs *= 2
            }
        }
    }

    /** For the button beside the error, so a desk is never stuck with an empty form. */
    fun retryReference() {
        _state.value = _state.value.copy(referenceError = null, referenceLoading = true)
        loadReference()
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
            val ticket = VmsClient.call { apis.current().beginRead() }

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
        } catch (e: ApiException) {
            _state.value = _state.value.copy(busy = null, error = e.message)
        } catch (e: CardReadException) {
            _state.value = _state.value.copy(busy = null, error = e.message)
        }
    }

    // ---------------------------------------------------------------- manual entry

    // ---------------------------------------------------------------- the camera

    fun openMrzScan() {
        _state.value = _state.value.copy(
            step = Step.MrzScan,
            busy = "Hold the card in the frame\u2026",
            error = null,
            mrzText = null,
            mrzComplete = false,
        )
    }

    fun closeMrzScan() {
        _state.value = _state.value.copy(step = Step.InsertCard, busy = null)
    }

    /**
     * One frame's text, checked by the server.
     *
     * A complete read - the three lines, every check digit holding - fills the form and moves
     * on. A partial one, where only the printed number survived, is kept but does not move on:
     * the officer sees the number appear and is told to turn the card over, because the back
     * carries everything and the front carries one field. Settling for the number while the
     * rest is in front of the camera is the fault this whole path exists to avoid.
     */
    fun onMrzText(text: String) {
        if (_state.value.step != Step.MrzScan) return

        viewModelScope.launch {
            /*  A failed frame is not an error to show.
             *
             *  This runs on whatever the camera happened to see, several times a second. A
             *  connection that blinked, or a frame that turned out not to be a card after
             *  all, is the ordinary case - and putting it on the screen would bury the one
             *  message that matters under a stream of ones that do not. The officer finds
             *  out the server is unreachable from the card read, which says so properly. */
            val result = try {
                VmsClient.call { apis.current().readMrz(MrzRequest(text)) }
            } catch (e: ApiException) {
                Log.d(TAG, "A frame could not be checked", e)
                return@launch
            }

            if (!result.ok || _state.value.step != Step.MrzScan) return@launch

            val identity = result.identity ?: return@launch

            if (!result.complete) {
                /* Kept, so a second or two later the officer can stop and use it if the back
                   will not read at all. Not advanced, so the back gets its chance first. */
                _state.value = _state.value.copy(
                    mrzText = text,
                    mrzComplete = false,
                    busy = "Read ${identity.idNumber} \u2014 now turn the card over for the " +
                        "name and dates.",
                )
                return@launch
            }

            _state.value = _state.value.copy(
                busy = null,
                card = null,
                readRequestId = null,
                mrzText = text,
                mrzComplete = true,
                manual = ManualDraft(
                    idNumber = identity.idNumber.orEmpty(),
                    cardNumber = identity.cardNumber.orEmpty(),
                    fullNameEnglish = identity.fullNameEnglish.orEmpty(),
                    nationalityEnglish = identity.nationalityEnglish.orEmpty(),
                    dateOfBirth = identity.dateOfBirth.orEmpty(),
                    expiryDate = identity.expiryDate.orEmpty(),
                ),
                error = null,
                step = Step.VisitorInformation,
            )
        }
    }

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
        /* Capped at the column, like every other typed field. A name typed past 200
           characters is not a name, and the server would cut it to fit in silence. */
        val capped = query.take(FieldRules.PersonToVisit)
        _state.value = _state.value.copy(hostQuery = capped, host = null)
        hostQueries.value = capped
    }

    fun selectHost(person: PersonDto) {
        _state.value = _state.value.copy(host = person, hostQuery = person.displayName, hostResults = emptyList())
    }

    fun setSearchAllEntities(all: Boolean) {
        _state.value = _state.value.copy(searchAllEntities = all)
        hostQueries.value = _state.value.hostQuery
    }

    fun setContactMobile(mobile: String) {
        _state.value = _state.value.copy(contactMobile = mobile.take(FieldRules.ContactMobile))
    }

    fun setPurpose(purpose: String) {
        _state.value = _state.value.copy(purpose = purpose)
    }

    fun setPurposeOther(text: String) {
        _state.value = _state.value.copy(purposeOther = text.take(FieldRules.PurposeOther))
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
                        apis.current().people(term, current.entityId, current.searchAllEntities)
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

        /* A last chance before the officer meets a form they cannot fill. The lists are
           needed here and nowhere else, and by now the server has had a visitor's worth of
           time to wake up. */
        if (_state.value.entities.isEmpty() && !_state.value.referenceLoading) loadReference()
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
            /*  A photographed card sends the text, not the fields.
             *
             *  The server parses it again at save time and takes the provenance from what it
             *  parsed - DigitalCard, below an unverified chip read, because the zone is
             *  printed and not signed. Sending the fields instead would make that a claim by
             *  the tablet, which is the thing this API is built not to accept. */
            mrzText = current.mrzText?.takeIf { current.mrzComplete },

            manual = current.manual?.takeIf { current.mrzText == null || !current.mrzComplete }?.let {
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
            personToVisitDirectoryId = current.host?.directoryObjectId,
            purpose = current.purpose!!,
            purposeOther = current.purposeOther.trim().ifBlank { null },
            contactMobile = current.contactMobile.trim().ifBlank { null },
        )

        try {
            val saved = VmsClient.call { apis.current().saveVisit(request) }
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
            entities = current.entities,
            purposes = current.purposes,
            otherPurpose = current.otherPurpose,
            entityId = current.entityId,
            searchAllEntities = current.searchAllEntities,
            reader = current.reader,
            server = current.server,
            serverIsDefault = current.serverIsDefault,
        )
        hostQueries.value = ""
    }

    fun dismissError() {
        _state.value = _state.value.copy(error = null)
    }

    /** Built by hand for the same reason the rest is: four dependencies, all of them
        already living on the Application. */
    class Factory(private val application: VmsApplication) : ViewModelProvider.Factory {
        @Suppress("UNCHECKED_CAST")
        override fun <T : ViewModel> create(modelClass: Class<T>): T =
            VisitorViewModel(
                application.apis,
                application.settings,
                application.pin,
                application.reader,
            ) as T
    }
}
