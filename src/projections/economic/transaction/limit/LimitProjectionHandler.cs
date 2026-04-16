using Whycespace.Projections.Economic.Transaction.Limit.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Transaction.Limit;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Transaction.Limit;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Transaction.Limit;

public sealed class LimitProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<LimitDefinedEventSchema>,
    IProjectionHandler<LimitCheckedEventSchema>,
    IProjectionHandler<LimitExceededEventSchema>
{
    private readonly PostgresProjectionStore<LimitReadModel> _store;

    public LimitProjectionHandler(PostgresProjectionStore<LimitReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            LimitDefinedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            LimitCheckedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            LimitExceededEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            _ => throw new InvalidOperationException(
                $"LimitProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public async Task HandleAsync(LimitDefinedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    public async Task HandleAsync(LimitCheckedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    public async Task HandleAsync(LimitExceededEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    private async Task Project(LimitDefinedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new LimitReadModel { LimitId = e.AggregateId };
        state = LimitProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "LimitDefinedEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task Project(LimitCheckedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new LimitReadModel { LimitId = e.AggregateId };
        state = LimitProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "LimitCheckedEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task Project(LimitExceededEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new LimitReadModel { LimitId = e.AggregateId };
        state = LimitProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "LimitExceededEvent", eventId, eventVersion, correlationId, ct);
    }
}
