using System.Security.Cryptography;
using System.Text;

namespace Whycespace.Shared.Primitives.Id;

public interface IIdGenerator
{
    string Generate(IdGenerationContext context);

    /// <summary>
    /// Generates a deterministic GUID from a seed string.
    /// Same seed always produces the same GUID across processes, machines, and restarts.
    /// Uses SHA-256 truncated to 16 bytes with UUID v5 version/variant bits.
    /// </summary>
    Guid DeterministicGuid(string seed) => DeterministicIdHelper.FromSeed(seed);

    /// <summary>
    /// Generates a deterministic GUID from composite seed parts.
    /// Convenience overload that joins parts with ':' separator.
    /// Example: DeterministicGuid("commandId", "workflow", "step0")
    /// </summary>
    Guid DeterministicGuid(params string[] seedParts) => DeterministicIdHelper.FromSeed(string.Join(":", seedParts));
}

public sealed class DefaultGuidGenerator : IIdGenerator
{
    public static readonly DefaultGuidGenerator Instance = new();

    public string Generate(IdGenerationContext context)
    {
        if (string.IsNullOrEmpty(context.DeterministicKey))
            throw new InvalidOperationException(
                "DeterministicKey is required. Non-deterministic ID generation is not permitted. " +
                "Provide a stable seed via IdGenerationContext.DeterministicKey.");

        return DeterministicIdHelper.FromSeed(context.DeterministicKey).ToString("N");
    }

    public Guid DeterministicGuid(string seed) => DeterministicIdHelper.FromSeed(seed);
    public Guid DeterministicGuid(params string[] seedParts) => DeterministicIdHelper.FromSeed(string.Join(":", seedParts));
}

/// <summary>
/// Deterministic GUID generation from seed strings using SHA-256.
/// Thread-safe, zero-allocation for the hash computation.
///
/// Guarantee: same seed → same GUID, always, everywhere.
/// </summary>
public static class DeterministicIdHelper
{
    // WBSM v3 namespace UUID — used as the "namespace" for UUID v5-style generation.
    // This ensures our deterministic GUIDs don't collide with GUIDs from other systems.
    private static readonly byte[] NamespaceBytes = new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890").ToByteArray();

    public static Guid FromSeed(string seed)
    {
        var seedBytes = Encoding.UTF8.GetBytes(seed);

        // Combine namespace + seed for domain separation
        var combined = new byte[NamespaceBytes.Length + seedBytes.Length];
        Buffer.BlockCopy(NamespaceBytes, 0, combined, 0, NamespaceBytes.Length);
        Buffer.BlockCopy(seedBytes, 0, combined, NamespaceBytes.Length, seedBytes.Length);

        var hash = SHA256.HashData(combined);

        // Take first 16 bytes and set UUID v5 version + variant bits
        var guidBytes = new byte[16];
        Buffer.BlockCopy(hash, 0, guidBytes, 0, 16);

        // Version 5 (name-based SHA): set bits 4-7 of byte 6 to 0101
        guidBytes[6] = (byte)((guidBytes[6] & 0x0F) | 0x50);
        // Variant 1 (RFC 9562): set bits 6-7 of byte 8 to 10
        guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

        return new Guid(guidBytes);
    }
}
