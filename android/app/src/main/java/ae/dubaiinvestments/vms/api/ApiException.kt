package ae.dubaiinvestments.vms.api

/**
 * A refusal from the server, with the message it wrote.
 *
 * The API answers a rejected read or an incomplete visit with ProblemDetails whose detail
 * is written for somebody standing at a desk - "This read has already been used, or took
 * too long. Read the card again." Passing that through is better than translating it here
 * into something vaguer.
 */
class ApiException(
    message: String,
    val status: Int? = null,
    /**
     * The ProblemDetails title, when the server sent one.
     *
     * Kept because the detail is written for the officer and must not be matched on, while
     * the title says which kind of refusal this was. The tablet needs that distinction: a
     * rejected card read sends it back to the card, and a refused field must not.
     */
    val title: String? = null,
) : Exception(message)
