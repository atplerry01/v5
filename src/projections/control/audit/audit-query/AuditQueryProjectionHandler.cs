using Whycespace.Projections.Control.Audit.AuditQuery.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.Audit.AuditQuery;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.Audit.AuditQuery;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.Audit.AuditQuery;

public sealed class AuditQueryProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<AuditQueryIssuedEventSchema>,
    IProjectionHandler<AuditQueryCompletedEventSchema>,
    IProjectionHandler<AuditQueryExpiredEventSchema>
{
    private readonly PostgresProjectionStore<AuditQueryReadModel> _store;

    public AuditQueryProjectionHandler(PostgresProjectionStore<AuditQueryReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            AuditQueryIssuedEventSchema e    => Project(e.AggregateId, s => AuditQueryProjectionReducer.Apply(s, e), "AuditQueryIssuedEvent",    envelope, cancellationToken),
            AuditQueryCompletedEventSchema e => Project(e.AggregateId, s => AuditQueryProjectionReducer.Apply(s, e), "AuditQueryCompletedEvent", envelope, cancellationToken),
            AuditQueryExpiredEventSchema e   => Project(e.AggregateId, s => AuditQueryProjectionReducer.Apply(s, e), "AuditQueryExpiredEvent",   envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"AuditQueryProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(AuditQueryIssuedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AuditQueryProjectionReducer.Apply(s, e), "AuditQueryIssuedEvent", null, ct);

    public Task HandleAsync(AuditQueryCompletedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AuditQueryProjectionReducer.Apply(s, e), "AuditQueryCompletedEvent", null, ct);

    public Task HandleAsync(AuditQueryExpiredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AuditQueryProjectionReducer.Apply(s, e), "AuditQueryExpiredEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<AuditQueryReadModel, AuditQueryReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new AuditQueryReadModel { QueryId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName,
            envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
