using DI.Vms.Domain.Common;

namespace DI.Vms.Domain.Entities;

/// <summary>
/// The acknowledgement captured on the tablet at check-in (BRD 7). Held in its own table so
/// the image is not loaded on every visit query.
/// </summary>
public class VisitorSignature : Entity
{
    public Guid VisitId { get; set; }
    public Visit? Visit { get; set; }

    /// <summary>PNG bytes of the signature drawn on the tablet.</summary>
    public required byte[] Image { get; set; }

    public DateTimeOffset CapturedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Identifies the tablet the signature was drawn on (BRD 19).</summary>
    public string? DeviceId { get; set; }
}
