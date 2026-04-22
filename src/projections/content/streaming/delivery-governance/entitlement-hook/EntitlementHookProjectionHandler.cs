using Whycespace.Projections.Content.Streaming.DeliveryGovernance.EntitlementHook.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.EntitlementHook;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Streaming.DeliveryGovernance.EntitlementHook;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Streaming.DeliveryGovernance.EntitlementHook;

public sealed class EntitlementHookProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<EntitlementHookRegisteredEventSchema>,
    IProjectionHandler<EntitlementQueriedEventSchema>,
    IProjectionHandler<EntitlementRefreshedEventSchema>,
    IProjectionHandler<EntitlementInvalidatedEventSchema>,
    IProjectionHandler<EntitlementFailureRecordedEventSchema>
{
    private readonly PostgresProjectionStore<EntitlementHookReadModel> _store;

    public EntitlementHookProjectionHandler(PostgresProjectionStore<EntitlementHookReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            EntitlementHookRegisteredEventSchema e => Project(e.AggregateId, s => EntitlementHookProjectionReducer.Apply(s, e), "EntitlementHookRegisteredEvent", envelope, cancellationToken),
            EntitlementQueriedEventSchema e => Project(e.AggregateId, s => EntitlementHookProjectionReducer.Apply(s, e), "EntitlementQueriedEvent", envelope, cancellationToken),
            EntitlementRefreshedEventSchema e => Project(e.AggregateId, s => EntitlementHookProjectionReducer.Apply(s, e), "EntitlementRefreshedEvent", envelope, cancellationToken),
            EntitlementInvalidatedEventSchema e => Project(e.AggregateId, s => EntitlementHookProjectionReducer.Apply(s, e), "EntitlementInvalidatedEvent", envelope, cancellationToken),
            EntitlementFailureRecordedEventSchema e => Project(e.AggregateId, s => EntitlementHookProjectionReducer.Apply(s, e), "EntitlementFailureRecordedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"EntitlementHookProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(EntitlementHookRegisteredEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => EntitlementHookProjectionReducer.Apply(s, e), "EntitlementHookRegisteredEvent", null, ct);
    public Task HandleAsync(EntitlementQueriedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => EntitlementHookProjectionReducer.Apply(s, e), "EntitlementQueriedEvent", null, ct);
    public Task HandleAsync(EntitlementRefreshedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => EntitlementHookProjectionReducer.Apply(s, e), "EntitlementRefreshedEvent", null, ct);
    public Task HandleAsync(EntitlementInvalidatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => EntitlementHookProjectionReducer.Apply(s, e), "EntitlementInvalidatedEvent", null, ct);
    public Task HandleAsync(EntitlementFailureRecordedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => EntitlementHookProjectionReducer.Apply(s, e), "EntitlementFailureRecordedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<EntitlementHookReadModel, EntitlementHookReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new EntitlementHookReadModel { HookId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
