using Whycespace.Projections.Control.Audit.AuditEvent.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.Audit.AuditEvent;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.Audit.AuditEvent;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.Audit.AuditEvent;

public sealed class AuditEventProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<AuditEventCapturedEventSchema>,
    IProjectionHandler<AuditEventSealedEventSchema>
{
    private readonly PostgresProjectionStore<AuditEventReadModel> _store;

    public AuditEventProjectionHandler(PostgresProjectionStore<AuditEventReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            AuditEventCapturedEventSchema e => Project(e.AggregateId, s => AuditEventProjectionReducer.Apply(s, e), "AuditEventCapturedEvent", envelope, cancellationToken),
            AuditEventSealedEventSchema e   => Project(e.AggregateId, s => AuditEventProjectionReducer.Apply(s, e), "AuditEventSealedEvent",   envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"AuditEventProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(AuditEventCapturedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AuditEventProjectionReducer.Apply(s, e), "AuditEventCapturedEvent", null, ct);

    public Task HandleAsync(AuditEventSealedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AuditEventProjectionReducer.Apply(s, e), "AuditEventSealedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<AuditEventReadModel, AuditEventReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new AuditEventReadModel { AuditEventId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName,
            envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
