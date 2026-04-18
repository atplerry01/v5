using System.Collections.Concurrent;

namespace Whycespace.Tests.Integration.EconomicSystem.Phase3Resilience.Shared;

/// <summary>
/// Phase 3 in-process tracer. Correlates dispatch spans by OperationId
/// (CommandId) so the Phase 3 observability tests can assert:
///
///   * every dispatched command emits a "received" → "processed" →
///     "persisted" chain of spans,
///   * every span carries the canonical correlation ids (command /
///     correlation / aggregate),
///   * spans are ordered and wall-clock monotonic per operation.
///
/// Intentionally transport-agnostic: the Phase 3 soak runner persists the
/// per-operation trace to <c>tests/reports/phase3/traces-*.json</c> for
/// cross-referencing in the validation report, but the test assertions
/// run entirely in-process.
/// </summary>
public sealed class Tracer
{
    private readonly ConcurrentDictionary<Guid, ConcurrentQueue<TraceSpan>> _byOperation = new();

    public void Record(Guid operationId, string stage, Guid correlationId, Guid aggregateId)
    {
        var queue = _byOperation.GetOrAdd(operationId, _ => new ConcurrentQueue<TraceSpan>());
        queue.Enqueue(new TraceSpan(operationId, stage, correlationId, aggregateId, DateTimeOffset.UtcNow.UtcTicks));
    }

    public IReadOnlyList<TraceSpan> SpansFor(Guid operationId) =>
        _byOperation.TryGetValue(operationId, out var queue) ? queue.ToArray() : Array.Empty<TraceSpan>();

    public IReadOnlyDictionary<Guid, IReadOnlyList<TraceSpan>> AllOperations() =>
        _byOperation.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<TraceSpan>)kvp.Value.ToArray());

    public int OperationCount => _byOperation.Count;
}

public sealed record TraceSpan(Guid OperationId, string Stage, Guid CorrelationId, Guid AggregateId, long UtcTicks);
