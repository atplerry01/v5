using Whycespace.Projections.Economic.Capital.Reserve.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Capital.Reserve;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Capital.Reserve;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Capital.Reserve;

public sealed class CapitalReserveProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ReserveCreatedEventSchema>,
    IProjectionHandler<ReserveReleasedEventSchema>,
    IProjectionHandler<ReserveExpiredEventSchema>
{
    private readonly PostgresProjectionStore<CapitalReserveReadModel> _store;

    public CapitalReserveProjectionHandler(PostgresProjectionStore<CapitalReserveReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ReserveCreatedEventSchema e => Project(e.AggregateId, s => CapitalReserveProjectionReducer.Apply(s, e), "ReserveCreatedEvent", envelope, cancellationToken),
            ReserveReleasedEventSchema e => Project(e.AggregateId, s => CapitalReserveProjectionReducer.Apply(s, e), "ReserveReleasedEvent", envelope, cancellationToken),
            ReserveExpiredEventSchema e => Project(e.AggregateId, s => CapitalReserveProjectionReducer.Apply(s, e), "ReserveExpiredEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"CapitalReserveProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ReserveCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalReserveProjectionReducer.Apply(s, e), "ReserveCreatedEvent", null, ct);

    public Task HandleAsync(ReserveReleasedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalReserveProjectionReducer.Apply(s, e), "ReserveReleasedEvent", null, ct);

    public Task HandleAsync(ReserveExpiredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalReserveProjectionReducer.Apply(s, e), "ReserveExpiredEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<CapitalReserveReadModel, CapitalReserveReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new CapitalReserveReadModel { ReserveId = aggregateId };
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
