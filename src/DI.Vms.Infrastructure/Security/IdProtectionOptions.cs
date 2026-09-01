namespace DI.Vms.Infrastructure.Security;

/// <summary>
/// Keys for ID-number protection. In production these must come from Azure Key Vault (or
/// the equivalent on DI private infrastructure), never from appsettings - a configuration
/// file sitting beside the database defeats encryption at rest entirely.
/// </summary>
public sealed class IdProtectionOptions
{
    public const string SectionName = "IdProtection";

    /// <summary>Base64-encoded 32-byte AES key.</summary>
    public string EncryptionKey { get; set; } = string.Empty;

    /// <summary>Base64-encoded HMAC pepper, 32 bytes or more.</summary>
    public string HashPepper { get; set; } = string.Empty;
}
