using Whycespace.Projections.Business.Entitlement.UsageControl.UsageRight.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.UsageRight;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Entitlement.UsageControl.UsageRight;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Entitlement.UsageControl.UsageRight;

public sealed class UsageRightProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<UsageRightCreatedEventSchema>,
    IProjectionHandler<UsageRightUsedEventSchema>,
    IProjectionHandler<UsageRightConsumedEventSchema>
{
    private readonly PostgresProjectionStore<UsageRightReadModel> _store;

    public UsageRightProjectionHandler(PostgresProjectionStore<UsageRightReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            UsageRightCreatedEventSchema e  => Project(e.AggregateId, s => UsageRightProjectionReducer.Apply(s, e), "UsageRightCreatedEvent",  envelope, cancellationToken),
            UsageRightUsedEventSchema e     => Project(e.AggregateId, s => UsageRightProjectionReducer.Apply(s, e), "UsageRightUsedEvent",     envelope, cancellationToken),
            UsageRightConsumedEventSchema e => Project(e.AggregateId, s => UsageRightProjectionReducer.Apply(s, e), "UsageRightConsumedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"UsageRightProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(UsageRightCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => UsageRightProjectionReducer.Apply(s, e), "UsageRightCreatedEvent", null, ct);

    public Task HandleAsync(UsageRightUsedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => UsageRightProjectionReducer.Apply(s, e), "UsageRightUsedEvent", null, ct);

    public Task HandleAsync(UsageRightConsumedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => UsageRightProjectionReducer.Apply(s, e), "UsageRightConsumedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<UsageRightReadModel, UsageRightReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new UsageRightReadModel { UsageRightId = aggregateId };
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
