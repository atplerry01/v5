namespace Whycespace.Shared.Contracts.Projections.Integration.OutboundEffect;

/// <summary>
/// R3.B.1 — read-model persistence for outbound-effect projections.
/// R4.B extends the contract with bounded list queries so the admin/operator
/// surface can inspect by status or by a small filter set. Query methods
/// MUST cap <paramref name="limit"/> at an implementation-defined ceiling
/// (1000) to bound operator-query blast radius.
/// </summary>
public interface IOutboundEffectProjectionStore
{
    Task<OutboundEffectReadModel?> GetAsync(Guid effectId);
    Task UpsertAsync(OutboundEffectReadModel model);

    /// <summary>
    /// R4.B — list effects matching <paramref name="status"/> (canonical values
    /// from <c>OutboundEffectQueueStatus</c>). Results ordered by last
    /// transition descending. <paramref name="limit"/> capped at 1000.
    /// </summary>
    Task<IReadOnlyList<OutboundEffectReadModel>> ListByStatusAsync(
        string status,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// R4.B — list effects with optional provider/effect-type/status filters.
    /// Null filter = no constraint. Results ordered by last transition
    /// descending. <paramref name="limit"/> capped at 1000.
    /// </summary>
    Task<IReadOnlyList<OutboundEffectReadModel>> ListAsync(
        string? providerId = null,
        string? effectType = null,
        string? status = null,
        int limit = 100,
        CancellationToken cancellationToken = default);
}
