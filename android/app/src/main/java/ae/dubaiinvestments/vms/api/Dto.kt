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

@Serializable
data class PersonDto(
    val id: Int,
    val displayName: String,
    val title: String? = null,
    val email: String? = null,
    val companyName: String? = null,
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
    val purpose: String,
    val purposeOther: String? = null,
)

/** ASP.NET's ProblemDetails, which is what the API answers a bad request with. */
@Serializable
data class ProblemDetails(
    val title: String? = null,
    val detail: String? = null,
    val status: Int? = null,
)
