using Whycespace.Projections.Business.Entitlement.UsageControl.Limit.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.Limit;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Entitlement.UsageControl.Limit;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Entitlement.UsageControl.Limit;

public sealed class LimitProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<LimitCreatedEventSchema>,
    IProjectionHandler<LimitEnforcedEventSchema>,
    IProjectionHandler<LimitBreachedEventSchema>
{
    private readonly PostgresProjectionStore<LimitReadModel> _store;

    public LimitProjectionHandler(PostgresProjectionStore<LimitReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            LimitCreatedEventSchema e  => Project(e.AggregateId, s => LimitProjectionReducer.Apply(s, e), "LimitCreatedEvent",  envelope, cancellationToken),
            LimitEnforcedEventSchema e => Project(e.AggregateId, s => LimitProjectionReducer.Apply(s, e), "LimitEnforcedEvent", envelope, cancellationToken),
            LimitBreachedEventSchema e => Project(e.AggregateId, s => LimitProjectionReducer.Apply(s, e), "LimitBreachedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"LimitProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(LimitCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => LimitProjectionReducer.Apply(s, e), "LimitCreatedEvent", null, ct);

    public Task HandleAsync(LimitEnforcedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => LimitProjectionReducer.Apply(s, e), "LimitEnforcedEvent", null, ct);

    public Task HandleAsync(LimitBreachedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => LimitProjectionReducer.Apply(s, e), "LimitBreachedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<LimitReadModel, LimitReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new LimitReadModel { LimitId = aggregateId };
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
