namespace DI.Vms.Domain.Enums;

/// <summary>Government-issued identification accepted at reception (BRD 3, Step 1).</summary>
public enum IdType
{
    EmiratesId = 1,
    Passport = 2,
    UaeDrivingLicence = 3,
    GccId = 4,
    OtherGovernmentId = 99
}
