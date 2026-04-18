using Whycespace.Projections.Economic.Revenue.Payout.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Revenue.Payout;

public sealed class PayoutExecutedProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<PayoutRequestedEventSchema>,
    IProjectionHandler<PayoutExecutedEventSchema>,
    IProjectionHandler<PayoutFailedEventSchema>,
    IProjectionHandler<PayoutCompensationRequestedEventSchema>,
    IProjectionHandler<PayoutCompensatedEventSchema>
{
    private readonly PostgresProjectionStore<PayoutReadModel> _store;

    public PayoutExecutedProjectionHandler(PostgresProjectionStore<PayoutReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            PayoutRequestedEventSchema e              => Project(e.AggregateId, s => PayoutProjectionReducer.Apply(s, e), "PayoutRequestedEvent", envelope, cancellationToken),
            PayoutExecutedEventSchema e               => Project(e.AggregateId, s => PayoutProjectionReducer.Apply(s, e), "PayoutExecutedEvent", envelope, cancellationToken),
            PayoutFailedEventSchema e                 => Project(e.AggregateId, s => PayoutProjectionReducer.Apply(s, e), "PayoutFailedEvent", envelope, cancellationToken),
            PayoutCompensationRequestedEventSchema e  => Project(e.AggregateId, s => PayoutProjectionReducer.Apply(s, e), "PayoutCompensationRequestedEvent", envelope, cancellationToken),
            PayoutCompensatedEventSchema e            => Project(e.AggregateId, s => PayoutProjectionReducer.Apply(s, e), "PayoutCompensatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"PayoutExecutedProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(PayoutRequestedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PayoutProjectionReducer.Apply(s, e), "PayoutRequestedEvent", null, ct);

    public Task HandleAsync(PayoutExecutedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PayoutProjectionReducer.Apply(s, e), "PayoutExecutedEvent", null, ct);

    public Task HandleAsync(PayoutFailedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PayoutProjectionReducer.Apply(s, e), "PayoutFailedEvent", null, ct);

    public Task HandleAsync(PayoutCompensationRequestedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PayoutProjectionReducer.Apply(s, e), "PayoutCompensationRequestedEvent", null, ct);

    public Task HandleAsync(PayoutCompensatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PayoutProjectionReducer.Apply(s, e), "PayoutCompensatedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<PayoutReadModel, PayoutReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new PayoutReadModel { PayoutId = aggregateId };
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
