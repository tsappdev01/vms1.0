package ae.dubaiinvestments.vms.api

import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.Query

/** The server's endpoints. See src/DI.Vms.Blazor/Api/VisitsApi.cs. */
interface VmsApi {

    /** Entities and purposes in one call - a tablet on office wifi should draw the form
        from one round trip, not three. */
    @GET("api/reference")
    suspend fun reference(): ReferenceDto

    /** Host search, run on the server. The list is 725 people with titles, emails and
        employers, and that is a staff directory not to be sitting on a tablet. */
    @GET("api/people")
    suspend fun people(
        @Query("q") q: String,
        @Query("entityId") entityId: Int?,
        @Query("allEntities") allEntities: Boolean,
    ): List<PersonDto>

    /** Start a read: the server issues an ID this device could not have chosen. */
    @POST("api/reads")
    suspend fun beginRead(): ReadTicketDto

    @POST("api/visits")
    suspend fun saveVisit(@Body request: SaveVisitRequest): SavedVisitDto
}
