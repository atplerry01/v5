using Whycespace.Projections.Economic.Compliance.Audit.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Compliance.Audit;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Compliance.Audit;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Compliance.Audit;

public sealed class AuditRecordProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<AuditRecordCreatedEventSchema>,
    IProjectionHandler<AuditRecordFinalizedEventSchema>
{
    private readonly PostgresProjectionStore<AuditRecordReadModel> _store;

    public AuditRecordProjectionHandler(PostgresProjectionStore<AuditRecordReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            AuditRecordCreatedEventSchema e => Project(e.AggregateId, s => AuditRecordProjectionReducer.Apply(s, e), "AuditRecordCreatedEvent", envelope, cancellationToken),
            AuditRecordFinalizedEventSchema e => Project(e.AggregateId, s => AuditRecordProjectionReducer.Apply(s, e), "AuditRecordFinalizedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"AuditRecordProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(AuditRecordCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AuditRecordProjectionReducer.Apply(s, e), "AuditRecordCreatedEvent", null, ct);

    public Task HandleAsync(AuditRecordFinalizedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AuditRecordProjectionReducer.Apply(s, e), "AuditRecordFinalizedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<AuditRecordReadModel, AuditRecordReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new AuditRecordReadModel { AuditRecordId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(
            aggregateId,
            state,
            eventTypeName,
            envelope?.EventId ?? Guid.Empty,
            envelope?.SequenceNumber ?? 0,
            envelope?.CorrelationId ?? Guid.Empty,
            ct);
    }
}
