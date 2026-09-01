using DI.Vms.Domain.Enums;

namespace DI.Vms.Application.Abstractions;

/// <summary>
/// Encrypts, hashes and masks government ID numbers (BRD 22). The plaintext exists
/// only inside implementations of this interface and the request that supplied it.
/// </summary>
public interface IIdProtector
{
    /// <summary>Encrypts the normalised ID number.</summary>
    byte[] Encrypt(string normalisedIdNumber);

    /// <summary>Decrypts. Callers must have checked permission and must write an audit row.</summary>
    string Decrypt(byte[] cipher);

    /// <summary>
    /// Keyed HMAC of the normalised ID number, for the lookup index. Keyed rather than a
    /// plain hash: the Emirates ID space is small and structured, so an unkeyed digest is
    /// brute-forceable and therefore equivalent to storing the number.
    /// </summary>
    byte[] Hash(string normalisedIdNumber);

    /// <summary>The display form, e.g. <c>784-XXXX-XXXXXXX-1</c>.</summary>
    string Mask(string normalisedIdNumber, IdType idType);
}
