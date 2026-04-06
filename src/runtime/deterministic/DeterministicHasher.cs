using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Whyce.Runtime.Deterministic;

/// <summary>
/// Deterministic hasher. All hashing in the runtime MUST go through this class
/// to ensure consistent, reproducible hash generation across executions.
///
/// Uses SHA256 exclusively. No timestamps in hash inputs.
/// Same input always produces the same hash.
/// </summary>
public static class DeterministicHasher
{
    /// <summary>
    /// Computes a SHA256 hash of the input string.
    /// Returns lowercase hex string.
    /// </summary>
    public static string ComputeHash(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(hash);
    }

    /// <summary>
    /// Computes a deterministic GUID from a seed string.
    /// Same seed always produces the same GUID.
    /// </summary>
    public static Guid ComputeGuid(string seed)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        return new Guid(hash.AsSpan(0, 16));
    }

    /// <summary>
    /// Computes a deterministic hash of a serialized object.
    /// </summary>
    public static string ComputePayloadHash(object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        return ComputeHash(json);
    }

    /// <summary>
    /// Computes a composite hash from multiple inputs.
    /// Order matters — different order produces different hash.
    /// </summary>
    public static string ComputeCompositeHash(params string[] inputs)
    {
        var composite = string.Join(":", inputs);
        return ComputeHash(composite);
    }
}
