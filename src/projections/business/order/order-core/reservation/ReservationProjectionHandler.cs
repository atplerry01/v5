using Whycespace.Projections.Business.Order.OrderCore.Reservation.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.Reservation;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Order.OrderCore.Reservation;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Order.OrderCore.Reservation;

public sealed class ReservationProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ReservationHeldEventSchema>,
    IProjectionHandler<ReservationConfirmedEventSchema>,
    IProjectionHandler<ReservationReleasedEventSchema>,
    IProjectionHandler<ReservationExpiredEventSchema>
{
    private readonly PostgresProjectionStore<ReservationReadModel> _store;

    public ReservationProjectionHandler(PostgresProjectionStore<ReservationReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ReservationHeldEventSchema e      => Project(e.AggregateId, s => ReservationProjectionReducer.Apply(s, e), "ReservationHeldEvent",      envelope, cancellationToken),
            ReservationConfirmedEventSchema e => Project(e.AggregateId, s => ReservationProjectionReducer.Apply(s, e), "ReservationConfirmedEvent", envelope, cancellationToken),
            ReservationReleasedEventSchema e  => Project(e.AggregateId, s => ReservationProjectionReducer.Apply(s, e), "ReservationReleasedEvent",  envelope, cancellationToken),
            ReservationExpiredEventSchema e   => Project(e.AggregateId, s => ReservationProjectionReducer.Apply(s, e), "ReservationExpiredEvent",   envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ReservationProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ReservationHeldEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ReservationProjectionReducer.Apply(s, e), "ReservationHeldEvent", null, ct);

    public Task HandleAsync(ReservationConfirmedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ReservationProjectionReducer.Apply(s, e), "ReservationConfirmedEvent", null, ct);

    public Task HandleAsync(ReservationReleasedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ReservationProjectionReducer.Apply(s, e), "ReservationReleasedEvent", null, ct);

    public Task HandleAsync(ReservationExpiredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ReservationProjectionReducer.Apply(s, e), "ReservationExpiredEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ReservationReadModel, ReservationReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ReservationReadModel { ReservationId = aggregateId };
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
