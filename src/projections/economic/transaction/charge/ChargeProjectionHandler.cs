using Whycespace.Projections.Economic.Transaction.Charge.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Transaction.Charge;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Transaction.Charge;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Transaction.Charge;

public sealed class ChargeProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ChargeCalculatedEventSchema>,
    IProjectionHandler<ChargeAppliedEventSchema>
{
    private readonly PostgresProjectionStore<ChargeReadModel> _store;

    public ChargeProjectionHandler(PostgresProjectionStore<ChargeReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ChargeCalculatedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            ChargeAppliedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ChargeProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public async Task HandleAsync(ChargeCalculatedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    public async Task HandleAsync(ChargeAppliedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    private async Task Project(ChargeCalculatedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new ChargeReadModel { ChargeId = e.AggregateId };
        state = ChargeProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "ChargeCalculatedEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task Project(ChargeAppliedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new ChargeReadModel { ChargeId = e.AggregateId };
        state = ChargeProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "ChargeAppliedEvent", eventId, eventVersion, correlationId, ct);
    }
}
