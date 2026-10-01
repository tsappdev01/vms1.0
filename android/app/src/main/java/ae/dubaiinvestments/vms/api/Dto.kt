package ae.dubaiinvestments.vms.api

import kotlinx.serialization.Serializable

/*  The wire types.

    They mirror the records in src/DI.Vms.Blazor/Api/VisitsApi.cs. The server serialises
    with ASP.NET's web defaults, which is camelCase, and kotlinx.serialization matches
    property names literally - so these names are the contract and renaming one silently
    sends null.

    Deliberately no card fields on the way up beyond the signed XML. A name in the request
    would be a name the server took on trust from a tablet; it parses its own out of the
    document instead, and the signature is what makes that worth anything. */

@Serializable
data class EntityDto(val id: Int, val name: String)

@Serializable
data class ReferenceDto(
    val entities: List<EntityDto>,
    val purposes: List<String>,
    val otherPurpose: String,
)

/** A host. No email address: the screen never showed one, and the server records the
    host's email from its own copy when the visit is saved.

    [id] is 0 for somebody the server found in the Entra ID directory but who has no
    vms.Person row yet - one is written when a visit to them is saved. [directoryObjectId]
    is what identifies them until then, so both go back in the save request and the server
    uses whichever it was given. */
@Serializable
data class PersonDto(
    val id: Int,
    val displayName: String,
    val title: String? = null,
    val companyName: String? = null,
    val directoryObjectId: String? = null,
)

@Serializable
data class ReadTicketDto(val requestId: String)

@Serializable
data class SavedVisitDto(
    val id: Int,
    val recordedAtUtc: String,
    val captureMethod: String,
    val warning: String? = null,
)

/** What reception typed when the chip would not read. */
@Serializable
data class ManualIdentity(
    val idNumber: String? = null,
    val cardNumber: String? = null,
    val fullNameEnglish: String? = null,
    val fullNameArabic: String? = null,
    val nationalityEnglish: String? = null,
    val dateOfBirth: String? = null,
    val expiryDate: String? = null,
    val addressMobile: String? = null,
)

@Serializable
data class SaveVisitRequest(
    val requestId: String? = null,
    val readResponseXml: String? = null,
    val manual: ManualIdentity? = null,
    val entityId: Int,
    val personToVisit: String,
    val personToVisitId: Int? = null,
    val personToVisitDirectoryId: String? = null,
    val purpose: String,
    val purposeOther: String? = null,
    /** The number the visitor gives at the desk. Optional, and last, so a server built
        before this ignores it rather than refusing the save. */
    val contactMobile: String? = null,
    /** The text the camera read off a card, when the visitor had no chip to insert. The
        server parses it and decides the provenance from what it parsed, which is why the
        fields are not sent alongside it. */
    val mrzText: String? = null,
)

/** One frame's worth of text, for `/api/mrz`. */
@Serializable
data class MrzRequest(val text: String)

/** What that text turned out to be.

    [complete] is false when only the printed ID number could be salvaged, which is the
    difference between a filled form and a filled ID field - and what lets the screen say
    "turn the card over" instead of settling. */
@Serializable
data class MrzResultDto(
    val ok: Boolean,
    val problem: String? = null,
    val complete: Boolean = false,
    val identity: ManualIdentity? = null,
    /** The identity carries fields read off the face of the card rather than out of a zone,
        so nothing in it but the ID number has been checked by arithmetic. True for the UAE
        Pass digital card, which has a QR code where a zone would be. Defaulted, so a server
        built before this simply never sets it. */
    val fromPrint: Boolean = false,
    /** The card itself cut the name short with an ellipsis, as UAE Pass does when it does not
        fit the box. The officer has to finish it, which is a different instruction from
        checking it. */
    val nameWasCut: Boolean = false,
)

/** ASP.NET's ProblemDetails, which is what the API answers a bad request with. */
@Serializable
data class ProblemDetails(
    val title: String? = null,
    val detail: String? = null,
    val status: Int? = null,
)
