namespace Whyce.Shared.Kernel.Determinism;

/// <summary>
/// HSID v2.1 — single source of truth for topology-aware, bucket-scoped,
/// bounded-sequence compact IDs of the form
/// <c>PPP-LLLL-TTT-TOPOLOGY-SEQ</c>.
///
/// This is a SECOND, parallel deterministic identity seam alongside
/// <see cref="Whyce.Shared.Kernel.Domain.IIdGenerator"/>. The two seams have
/// non-overlapping responsibilities — see
/// <c>claude/new-rules/20260407-200000-hsid-v2.1-parallel-seam.md</c>.
/// </summary>
public interface IDeterministicIdEngine
{
    /// <summary>
    /// Generate a compact deterministic ID. Inputs MUST be deterministic;
    /// the engine MUST NOT read the system clock or RNG.
    /// </summary>
    string Generate(
        IdPrefix prefix,
        LocationCode location,
        TopologyCode topology,
        string seed,
        int sequence);

    /// <summary>
    /// Structural validation of an HSID against the canonical
    /// <c>PPP-LLLL-TTT-TOPOLOGY-SEQ</c> shape. Does not verify topology
    /// membership or sequence monotonicity — only the format envelope.
    /// </summary>
    bool IsValid(string id);
}
