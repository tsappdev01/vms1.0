namespace DI.Vms.Domain.Enums;

/// <summary>Visitor categories used for reporting (BRD 15).</summary>
public enum VisitorType
{
    Guest = 1,
    Customer = 2,
    Supplier = 3,
    Contractor = 4,
    Consultant = 5,
    GovernmentOfficial = 6,
    InterviewCandidate = 7,
    Delivery = 8,
    ServiceProvider = 9,
    Other = 99
}
