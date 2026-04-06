using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Whyce.Engines.T0U.WhyceChain.Hashing;

/// <summary>
/// Deterministic chain hasher. Implements the canonical hash rule:
/// Hash = SHA256(previousHash + payloadHash + sequence)
/// NO timestamps allowed in hash computation.
/// </summary>
public static class ChainHasher
{
    /// <summary>
    /// Computes block hash: SHA256(previousHash + payloadHash + sequence).
    /// This is the canonical chain hash function.
    /// </summary>
    public static string ComputeBlockHash(string previousHash, string payloadHash, long sequence)
    {
        var input = $"{previousHash}{payloadHash}{sequence}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }

    /// <summary>
    /// Computes deterministic event payload hash from a list of events.
    /// </summary>
    public static string ComputePayloadHash(IReadOnlyList<object> events)
    {
        var json = JsonSerializer.Serialize(events);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexStringLower(bytes);
    }

    /// <summary>
    /// Computes a deterministic event hash combining event type and payload.
    /// </summary>
    public static string ComputeEventHash(object evt, int sequenceIndex)
    {
        var eventType = evt.GetType().Name;
        var payload = JsonSerializer.Serialize(evt);
        var input = $"{eventType}:{sequenceIndex}:{payload}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }

    /// <summary>
    /// Computes the genesis block hash (first block in the chain).
    /// </summary>
    public static string ComputeGenesisHash()
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes("genesis:0"));
        return Convert.ToHexStringLower(bytes);
    }
}
