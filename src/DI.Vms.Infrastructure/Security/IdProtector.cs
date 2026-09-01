using System.Security.Cryptography;
using System.Text;
using DI.Vms.Application.Abstractions;
using DI.Vms.Domain.Enums;
using DI.Vms.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace DI.Vms.Infrastructure.Security;

/// <summary>
/// AES-256-GCM for confidentiality and integrity, and a keyed HMAC for the lookup index
/// (BRD 22).
/// </summary>
public sealed class IdProtector : IIdProtector
{
    private const int NonceSize = 12;   // 96-bit nonce, the size AES-GCM is specified for
    private const int TagSize = 16;     // 128-bit authentication tag

    private readonly byte[] _key;
    private readonly byte[] _pepper;

    public IdProtector(IOptions<IdProtectionOptions> options)
    {
        var o = options.Value;

        if (string.IsNullOrWhiteSpace(o.EncryptionKey) || string.IsNullOrWhiteSpace(o.HashPepper))
        {
            throw new InvalidOperationException(
                "IdProtection:EncryptionKey and IdProtection:HashPepper must be configured. " +
                "Generate development values with: openssl rand -base64 32");
        }

        _key = Convert.FromBase64String(o.EncryptionKey);
        _pepper = Convert.FromBase64String(o.HashPepper);

        if (_key.Length != 32)
        {
            throw new InvalidOperationException(
                $"IdProtection:EncryptionKey must decode to 32 bytes for AES-256; got {_key.Length}.");
        }

        if (_pepper.Length < 32)
        {
            throw new InvalidOperationException(
                $"IdProtection:HashPepper must decode to at least 32 bytes; got {_pepper.Length}.");
        }
    }

    /// <summary>Layout: nonce (12) || tag (16) || ciphertext.</summary>
    public byte[] Encrypt(string normalisedIdNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalisedIdNumber);

        var plaintext = Encoding.UTF8.GetBytes(normalisedIdNumber);
        var result = new byte[NonceSize + TagSize + plaintext.Length];

        var nonce = result.AsSpan(0, NonceSize);
        var tag = result.AsSpan(NonceSize, TagSize);
        var ciphertext = result.AsSpan(NonceSize + TagSize);

        RandomNumberGenerator.Fill(nonce);

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        CryptographicOperations.ZeroMemory(plaintext);
        return result;
    }

    public string Decrypt(byte[] cipher)
    {
        ArgumentNullException.ThrowIfNull(cipher);

        if (cipher.Length < NonceSize + TagSize)
        {
            throw new CryptographicException("Ciphertext is too short to be valid.");
        }

        var nonce = cipher.AsSpan(0, NonceSize);
        var tag = cipher.AsSpan(NonceSize, TagSize);
        var ciphertext = cipher.AsSpan(NonceSize + TagSize);
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        var value = Encoding.UTF8.GetString(plaintext);
        CryptographicOperations.ZeroMemory(plaintext);
        return value;
    }

    public byte[] Hash(string normalisedIdNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalisedIdNumber);
        return HMACSHA256.HashData(_pepper, Encoding.UTF8.GetBytes(normalisedIdNumber));
    }

    public string Mask(string normalisedIdNumber, IdType idType) =>
        IdNumber.Mask(normalisedIdNumber, idType);
}
