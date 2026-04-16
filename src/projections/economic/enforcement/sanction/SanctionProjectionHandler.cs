using Whycespace.Projections.Economic.Enforcement.Sanction.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Enforcement.Sanction;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Enforcement.Sanction;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Enforcement.Sanction;

public sealed class SanctionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<SanctionIssuedEventSchema>,
    IProjectionHandler<SanctionActivatedEventSchema>,
    IProjectionHandler<SanctionExpiredEventSchema>,
    IProjectionHandler<SanctionRevokedEventSchema>
{
    private readonly PostgresProjectionStore<SanctionReadModel> _store;

    public SanctionProjectionHandler(PostgresProjectionStore<SanctionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            SanctionIssuedEventSchema e    => Project(e.AggregateId, s => SanctionProjectionReducer.Apply(s, e), "SanctionIssuedEvent", envelope, cancellationToken),
            SanctionActivatedEventSchema e => Project(e.AggregateId, s => SanctionProjectionReducer.Apply(s, e), "SanctionActivatedEvent", envelope, cancellationToken),
            SanctionExpiredEventSchema e   => Project(e.AggregateId, s => SanctionProjectionReducer.Apply(s, e), "SanctionExpiredEvent", envelope, cancellationToken),
            SanctionRevokedEventSchema e   => Project(e.AggregateId, s => SanctionProjectionReducer.Apply(s, e), "SanctionRevokedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"SanctionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(SanctionIssuedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SanctionProjectionReducer.Apply(s, e), "SanctionIssuedEvent", null, ct);

    public Task HandleAsync(SanctionActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SanctionProjectionReducer.Apply(s, e), "SanctionActivatedEvent", null, ct);

    public Task HandleAsync(SanctionExpiredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SanctionProjectionReducer.Apply(s, e), "SanctionExpiredEvent", null, ct);

    public Task HandleAsync(SanctionRevokedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SanctionProjectionReducer.Apply(s, e), "SanctionRevokedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<SanctionReadModel, SanctionReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new SanctionReadModel { SanctionId = aggregateId };
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
