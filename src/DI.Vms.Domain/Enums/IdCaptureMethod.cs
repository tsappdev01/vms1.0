namespace DI.Vms.Domain.Enums;

/// <summary>
/// How the identity data reached the system. BRD 21 deliberately leaves the reading
/// mechanism open, so the domain records which method was actually used rather than
/// assuming a single SDK.
/// </summary>
public enum IdCaptureMethod
{
    /// <summary>Typed in by the security officer; no automated extraction.</summary>
    Manual = 0,
    Ocr = 1,
    Mrz = 2,
    Nfc = 3,
    BarcodeOrQr = 4,
    CardReader = 5
}
