using Whycespace.Projections.Platform.Routing.DispatchRule.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Platform.Routing.DispatchRule;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Platform.Routing.DispatchRule;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Platform.Routing.DispatchRule;

public sealed class DispatchRuleProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<DispatchRuleRegisteredEventSchema>,
    IProjectionHandler<DispatchRuleDeactivatedEventSchema>
{
    private readonly PostgresProjectionStore<DispatchRuleReadModel> _store;

    public DispatchRuleProjectionHandler(PostgresProjectionStore<DispatchRuleReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            DispatchRuleRegisteredEventSchema e => Project(e.AggregateId, s => DispatchRuleProjectionReducer.Apply(s, e, envelope.Timestamp), "DispatchRuleRegisteredEvent", envelope, cancellationToken),
            DispatchRuleDeactivatedEventSchema e => Project(e.AggregateId, s => DispatchRuleProjectionReducer.Apply(s, e, envelope.Timestamp), "DispatchRuleDeactivatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException($"DispatchRuleProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(DispatchRuleRegisteredEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => DispatchRuleProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "DispatchRuleRegisteredEvent", null, ct);
    public Task HandleAsync(DispatchRuleDeactivatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => DispatchRuleProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "DispatchRuleDeactivatedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<DispatchRuleReadModel, DispatchRuleReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new DispatchRuleReadModel { DispatchRuleId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
