using System.Security.Cryptography;
using System.Text;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Runtime.Resilience;

/// <summary>
/// R2.A.1 canonical <see cref="IRandomProvider"/> implementation. Derives
/// bounded random values via SHA256(seed) — same seed yields the same
/// output across processes, instances, and replays. Mirrors the existing
/// <c>DeterministicIdGenerator</c> pattern.
///
/// No hidden RNG state. No clock. No <c>Random</c> class. The per-value
/// extraction reads 8 bytes of the SHA256 digest as the source.
///
/// Lives under <c>src/runtime/resilience/</c> (not host composition)
/// because it has zero host-specific dependencies and is co-located
/// with its first consumer, <see cref="DeterministicRetryExecutor"/>.
/// Host composition registers it via DI in R2.A.2 onward.
/// </summary>
public sealed class DeterministicRandomProvider : IRandomProvider
{
    public double NextDouble(string seed)
    {
        if (string.IsNullOrEmpty(seed))
            throw new ArgumentException("Seed is required.", nameof(seed));

        var u64 = DigestUlong(seed);
        // Take 53 bits — the precision of a double's mantissa — and divide
        // by 2^53 to produce a uniform value in [0.0, 1.0).
        var significand = u64 >> 11;
        return significand / (double)(1UL << 53);
    }

    public int NextInt(string seed, int minInclusive, int maxExclusive)
    {
        if (string.IsNullOrEmpty(seed))
            throw new ArgumentException("Seed is required.", nameof(seed));
        if (maxExclusive <= minInclusive)
            throw new ArgumentException(
                $"maxExclusive ({maxExclusive}) must be greater than minInclusive ({minInclusive}).",
                nameof(maxExclusive));

        var range = (uint)(maxExclusive - minInclusive);
        var u32 = (uint)DigestUlong(seed);
        return minInclusive + (int)(u32 % range);
    }

    public long NextLong(string seed)
    {
        if (string.IsNullOrEmpty(seed))
            throw new ArgumentException("Seed is required.", nameof(seed));

        return unchecked((long)DigestUlong(seed));
    }

    private static ulong DigestUlong(string seed)
    {
        Span<byte> digest = stackalloc byte[32];
        SHA256.HashData(Encoding.UTF8.GetBytes(seed), digest);

        // First 8 bytes of the digest, big-endian → ulong.
        ulong value = 0;
        for (int i = 0; i < 8; i++)
        {
            value = (value << 8) | digest[i];
        }
        return value;
    }
}
