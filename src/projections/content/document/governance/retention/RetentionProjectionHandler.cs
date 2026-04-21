using Whycespace.Projections.Content.Document.Governance.Retention.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Document.Governance.Retention;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Document.Governance.Retention;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Document.Governance.Retention;

public sealed class RetentionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<RetentionAppliedEventSchema>,
    IProjectionHandler<RetentionHoldPlacedEventSchema>,
    IProjectionHandler<RetentionReleasedEventSchema>,
    IProjectionHandler<RetentionExpiredEventSchema>,
    IProjectionHandler<RetentionMarkedEligibleForDestructionEventSchema>,
    IProjectionHandler<RetentionArchivedEventSchema>
{
    private readonly PostgresProjectionStore<RetentionReadModel> _store;

    public RetentionProjectionHandler(PostgresProjectionStore<RetentionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            RetentionAppliedEventSchema e => Project(e.AggregateId, s => RetentionProjectionReducer.Apply(s, e), "RetentionAppliedEvent", envelope, cancellationToken),
            RetentionHoldPlacedEventSchema e => Project(e.AggregateId, s => RetentionProjectionReducer.Apply(s, e), "RetentionHoldPlacedEvent", envelope, cancellationToken),
            RetentionReleasedEventSchema e => Project(e.AggregateId, s => RetentionProjectionReducer.Apply(s, e), "RetentionReleasedEvent", envelope, cancellationToken),
            RetentionExpiredEventSchema e => Project(e.AggregateId, s => RetentionProjectionReducer.Apply(s, e), "RetentionExpiredEvent", envelope, cancellationToken),
            RetentionMarkedEligibleForDestructionEventSchema e => Project(e.AggregateId, s => RetentionProjectionReducer.Apply(s, e), "RetentionMarkedEligibleForDestructionEvent", envelope, cancellationToken),
            RetentionArchivedEventSchema e => Project(e.AggregateId, s => RetentionProjectionReducer.Apply(s, e), "RetentionArchivedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"RetentionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(RetentionAppliedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RetentionProjectionReducer.Apply(s, e), "RetentionAppliedEvent", null, ct);

    public Task HandleAsync(RetentionHoldPlacedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RetentionProjectionReducer.Apply(s, e), "RetentionHoldPlacedEvent", null, ct);

    public Task HandleAsync(RetentionReleasedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RetentionProjectionReducer.Apply(s, e), "RetentionReleasedEvent", null, ct);

    public Task HandleAsync(RetentionExpiredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RetentionProjectionReducer.Apply(s, e), "RetentionExpiredEvent", null, ct);

    public Task HandleAsync(RetentionMarkedEligibleForDestructionEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RetentionProjectionReducer.Apply(s, e), "RetentionMarkedEligibleForDestructionEvent", null, ct);

    public Task HandleAsync(RetentionArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RetentionProjectionReducer.Apply(s, e), "RetentionArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<RetentionReadModel, RetentionReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new RetentionReadModel { RetentionId = aggregateId };
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
