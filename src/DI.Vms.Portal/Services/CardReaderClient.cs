using System.Text.Json.Serialization;
using Microsoft.JSInterop;

namespace DI.Vms.Portal.Services;

/// <summary>
/// Reads an Emirates ID through DI.Vms.CardBridge.
///
/// The bridge listens on the reception machine's loopback, which the Blazor server
/// cannot reach - so the calls are made from the browser, which is on that machine, and
/// the result crosses the circuit. That is why this goes through JS interop rather than
/// HttpClient.
/// </summary>
public sealed class CardReaderClient(IJSRuntime js)
{
    public async Task<ReaderStatus> GetStatusAsync()
    {
        try
        {
            return await js.InvokeAsync<ReaderStatus>("vms.cardReader.status")
                ?? new ReaderStatus { Detail = "No response from the bridge." };
        }
        catch (Exception ex)
        {
            return new ReaderStatus { Detail = ex.Message };
        }
    }

    /// <summary>Reads the chip. Throws with a message suitable for a security officer.</summary>
    public async Task<CardRead> ReadAsync()
    {
        var read = await js.InvokeAsync<CardRead>("vms.cardReader.read");
        return read ?? throw new InvalidOperationException("The reader returned nothing.");
    }

    public sealed class ReaderStatus
    {
        public bool Available { get; set; }
        public string? ReaderName { get; set; }
        public string Detail { get; set; } = string.Empty;
        public string? ToolkitVersion { get; set; }
        public string? LicenseExpiry { get; set; }
    }

    public sealed class CardRead
    {
        public string IdNumber { get; set; } = string.Empty;
        public string? CardNumber { get; set; }
        public string? Name { get; set; }
        public string? Nationality { get; set; }
        public string? DateOfBirth { get; set; }
        public string? ExpiryDate { get; set; }
        public string? Gender { get; set; }
        public string? PhotoBase64 { get; set; }
        public string? SignatureWarning { get; set; }
        public CardVerification? Verification { get; set; }

        /// <summary>
        /// The toolkit reports dates as DD/MM/YYYY. Misreading one for the other silently
        /// shifts a date rather than failing, so the conversion is explicit and returns
        /// null when the format is not what we expect.
        /// </summary>
        public DateOnly? ParseDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            if (DateOnly.TryParseExact(value.Trim(), "dd/MM/yyyy", null,
                    System.Globalization.DateTimeStyles.None, out var slashed))
            {
                return slashed;
            }

            return DateOnly.TryParseExact(value.Trim()[..Math.Min(10, value.Trim().Length)], "yyyy-MM-dd", null,
                    System.Globalization.DateTimeStyles.None, out var iso)
                ? iso
                : null;
        }
    }

    public sealed class CardVerification
    {
        public bool? IsGenuine { get; set; }
        public string CardStatus { get; set; } = "Unknown";
        public bool VgAvailable { get; set; }
        public string? VerifiedAtUtc { get; set; }

        [JsonPropertyName("verificationError")]
        public string? VerificationError { get; set; }

        [JsonPropertyName("statusError")]
        public string? StatusError { get; set; }
    }
}
