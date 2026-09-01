namespace DI.Vms.Domain.Common;

/// <summary>Raised when an operation would violate a business rule.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
