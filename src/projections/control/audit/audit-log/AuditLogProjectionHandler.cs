using Whycespace.Projections.Control.Audit.AuditLog.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.Audit.AuditLog;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.Audit.AuditLog;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.Audit.AuditLog;

public sealed class AuditLogProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<AuditEntryRecordedEventSchema>
{
    private readonly PostgresProjectionStore<AuditLogReadModel> _store;

    public AuditLogProjectionHandler(PostgresProjectionStore<AuditLogReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            AuditEntryRecordedEventSchema e => Project(e.AggregateId, s => AuditLogProjectionReducer.Apply(s, e), "AuditEntryRecordedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"AuditLogProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(AuditEntryRecordedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AuditLogProjectionReducer.Apply(s, e), "AuditEntryRecordedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<AuditLogReadModel, AuditLogReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new AuditLogReadModel { AuditLogId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName,
            envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
