using System.Collections.Concurrent;
using Whycespace.Shared.Contracts.Projections.Integration.OutboundEffect;

namespace Whycespace.Platform.Host.Adapters.OutboundEffects;

/// <summary>
/// R3.B.1 — in-memory read-model store for outbound effects. Placeholder while
/// the Postgres-backed store is wired (deferred to the R4 operator surface
/// pass, consistent with <see cref="WorkflowExecutionBootstrap"/>'s use of an
/// in-memory store at R3.A.6 landing).
/// </summary>
public sealed class InMemoryOutboundEffectProjectionStore : IOutboundEffectProjectionStore
{
    private readonly ConcurrentDictionary<Guid, OutboundEffectReadModel> _store = new();

    public Task<OutboundEffectReadModel?> GetAsync(Guid effectId)
    {
        _store.TryGetValue(effectId, out var model);
        return Task.FromResult(model);
    }

    public Task UpsertAsync(OutboundEffectReadModel model)
    {
        _store[model.EffectId] = model;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<OutboundEffectReadModel>> ListByStatusAsync(
        string status,
        int limit = 100,
        CancellationToken cancellationToken = default)
        => ListAsync(providerId: null, effectType: null, status: status, limit: limit, cancellationToken);

    public Task<IReadOnlyList<OutboundEffectReadModel>> ListAsync(
        string? providerId = null,
        string? effectType = null,
        string? status = null,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var effectiveLimit = Math.Min(Math.Max(1, limit), 1000);
        var results = _store.Values
            .Where(m => providerId is null || string.Equals(m.ProviderId, providerId, StringComparison.Ordinal))
            .Where(m => effectType is null || string.Equals(m.EffectType, effectType, StringComparison.Ordinal))
            .Where(m => status is null || string.Equals(m.Status, status, StringComparison.Ordinal))
            .OrderByDescending(m => m.LastTransitionAt ?? DateTimeOffset.MinValue)
            .Take(effectiveLimit)
            .ToList();
        return Task.FromResult<IReadOnlyList<OutboundEffectReadModel>>(results);
    }
}
