using System.Security.Cryptography;
using System.Text;

namespace Whyce.Engines.T0U.Determinism.Time;

/// <summary>
/// Replay-stable bucket: SHA256(seed)[..3] uppercase hex.
/// Width matches the locked HSID v2.1 TTT segment (3 chars).
/// No clock, no RNG.
/// </summary>
public sealed class DeterministicTimeBucketProvider : ITimeBucketProvider
{
    public string GetBucket(string seed)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        return Convert.ToHexString(bytes)[..3];
    }
}
