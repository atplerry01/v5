namespace Whycespace.Shared.Kernel.Domain;

/// <summary>
/// Deterministic randomness seam. Mirrors <see cref="IClock"/> / <see cref="IIdGenerator"/>:
/// the ONLY permitted source of random values in domain / engine / runtime / adapter paths.
/// All methods are seed-driven so replays and chaos tests are reproducible — no hidden RNG state.
///
/// Use cases (R2 onward): retry backoff jitter, partition-rebalance tiebreaks,
/// circuit-breaker half-open probe selection, load-shedding victim selection.
///
/// The seed MUST be derived from the operation's deterministic coordinates —
/// e.g. $"{correlationId}:retry:{attempt}" for retry jitter, or $"{consumerGroup}:rebalance:{epoch}"
/// for a rebalance decision. Random or wall-clock seeds defeat the purpose.
/// </summary>
public interface IRandomProvider
{
    /// <summary>Derives a double in [0.0, 1.0) from the given seed.</summary>
    double NextDouble(string seed);

    /// <summary>Derives an int in [minInclusive, maxExclusive) from the given seed.</summary>
    int NextInt(string seed, int minInclusive, int maxExclusive);

    /// <summary>Derives a long from the given seed.</summary>
    long NextLong(string seed);
}
