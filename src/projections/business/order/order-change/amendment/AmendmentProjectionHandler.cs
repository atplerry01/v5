using Whycespace.Projections.Business.Order.OrderChange.Amendment.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Order.OrderChange.Amendment;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Order.OrderChange.Amendment;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Order.OrderChange.Amendment;

public sealed class AmendmentProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<AmendmentRequestedEventSchema>,
    IProjectionHandler<AmendmentAcceptedEventSchema>,
    IProjectionHandler<AmendmentAppliedEventSchema>,
    IProjectionHandler<AmendmentRejectedEventSchema>,
    IProjectionHandler<AmendmentCancelledEventSchema>
{
    private readonly PostgresProjectionStore<AmendmentReadModel> _store;

    public AmendmentProjectionHandler(PostgresProjectionStore<AmendmentReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            AmendmentRequestedEventSchema e => Project(e.AggregateId, s => AmendmentProjectionReducer.Apply(s, e), "AmendmentRequestedEvent", envelope, cancellationToken),
            AmendmentAcceptedEventSchema  e => Project(e.AggregateId, s => AmendmentProjectionReducer.Apply(s, e), "AmendmentAcceptedEvent",  envelope, cancellationToken),
            AmendmentAppliedEventSchema   e => Project(e.AggregateId, s => AmendmentProjectionReducer.Apply(s, e), "AmendmentAppliedEvent",   envelope, cancellationToken),
            AmendmentRejectedEventSchema  e => Project(e.AggregateId, s => AmendmentProjectionReducer.Apply(s, e), "AmendmentRejectedEvent",  envelope, cancellationToken),
            AmendmentCancelledEventSchema e => Project(e.AggregateId, s => AmendmentProjectionReducer.Apply(s, e), "AmendmentCancelledEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"AmendmentProjectionHandler (order-change) received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(AmendmentRequestedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AmendmentProjectionReducer.Apply(s, e), "AmendmentRequestedEvent", null, ct);

    public Task HandleAsync(AmendmentAcceptedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AmendmentProjectionReducer.Apply(s, e), "AmendmentAcceptedEvent", null, ct);

    public Task HandleAsync(AmendmentAppliedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AmendmentProjectionReducer.Apply(s, e), "AmendmentAppliedEvent", null, ct);

    public Task HandleAsync(AmendmentRejectedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AmendmentProjectionReducer.Apply(s, e), "AmendmentRejectedEvent", null, ct);

    public Task HandleAsync(AmendmentCancelledEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AmendmentProjectionReducer.Apply(s, e), "AmendmentCancelledEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<AmendmentReadModel, AmendmentReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new AmendmentReadModel { AmendmentId = aggregateId };
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
