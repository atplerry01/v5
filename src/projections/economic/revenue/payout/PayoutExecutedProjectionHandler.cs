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
    IProjectionHandler<PayoutExecutedEventSchema>
{
    private readonly PostgresProjectionStore<PayoutReadModel> _store;

    public PayoutExecutedProjectionHandler(PostgresProjectionStore<PayoutReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            PayoutExecutedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            _ => throw new InvalidOperationException(
                $"PayoutExecutedProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public async Task HandleAsync(PayoutExecutedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    private async Task Project(PayoutExecutedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new PayoutReadModel { PayoutId = e.AggregateId };
        state = PayoutProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "PayoutExecutedEvent", eventId, eventVersion, correlationId, ct);
    }
}
