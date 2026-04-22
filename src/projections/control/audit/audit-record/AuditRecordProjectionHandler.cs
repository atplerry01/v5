using Whycespace.Projections.Control.Audit.AuditRecord.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.Audit.AuditRecord;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.Audit.AuditRecord;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.Audit.AuditRecord;

public sealed class AuditRecordProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<AuditRecordRaisedEventSchema>,
    IProjectionHandler<AuditRecordResolvedEventSchema>
{
    private readonly PostgresProjectionStore<AuditRecordReadModel> _store;

    public AuditRecordProjectionHandler(PostgresProjectionStore<AuditRecordReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            AuditRecordRaisedEventSchema e   => Project(e.AggregateId, s => AuditRecordProjectionReducer.Apply(s, e), "AuditRecordRaisedEvent",   envelope, cancellationToken),
            AuditRecordResolvedEventSchema e => Project(e.AggregateId, s => AuditRecordProjectionReducer.Apply(s, e), "AuditRecordResolvedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"AuditRecordProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(AuditRecordRaisedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AuditRecordProjectionReducer.Apply(s, e), "AuditRecordRaisedEvent", null, ct);

    public Task HandleAsync(AuditRecordResolvedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AuditRecordProjectionReducer.Apply(s, e), "AuditRecordResolvedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<AuditRecordReadModel, AuditRecordReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new AuditRecordReadModel { RecordId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName,
            envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
