using Whycespace.Projections.Control.Audit.AuditTrace.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.Audit.AuditTrace;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.Audit.AuditTrace;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.Audit.AuditTrace;

public sealed class AuditTraceProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<AuditTraceOpenedEventSchema>,
    IProjectionHandler<AuditTraceEventLinkedEventSchema>,
    IProjectionHandler<AuditTraceClosedEventSchema>
{
    private readonly PostgresProjectionStore<AuditTraceReadModel> _store;

    public AuditTraceProjectionHandler(PostgresProjectionStore<AuditTraceReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            AuditTraceOpenedEventSchema e      => Project(e.AggregateId, s => AuditTraceProjectionReducer.Apply(s, e), "AuditTraceOpenedEvent",      envelope, cancellationToken),
            AuditTraceEventLinkedEventSchema e => Project(e.AggregateId, s => AuditTraceProjectionReducer.Apply(s, e), "AuditTraceEventLinkedEvent", envelope, cancellationToken),
            AuditTraceClosedEventSchema e      => Project(e.AggregateId, s => AuditTraceProjectionReducer.Apply(s, e), "AuditTraceClosedEvent",      envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"AuditTraceProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(AuditTraceOpenedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AuditTraceProjectionReducer.Apply(s, e), "AuditTraceOpenedEvent", null, ct);

    public Task HandleAsync(AuditTraceEventLinkedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AuditTraceProjectionReducer.Apply(s, e), "AuditTraceEventLinkedEvent", null, ct);

    public Task HandleAsync(AuditTraceClosedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AuditTraceProjectionReducer.Apply(s, e), "AuditTraceClosedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<AuditTraceReadModel, AuditTraceReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new AuditTraceReadModel { TraceId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName,
            envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
