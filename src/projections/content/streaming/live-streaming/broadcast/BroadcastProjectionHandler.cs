using Whycespace.Projections.Content.Streaming.LiveStreaming.Broadcast.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.Broadcast;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Streaming.LiveStreaming.Broadcast;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Streaming.LiveStreaming.Broadcast;

public sealed class BroadcastProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<BroadcastCreatedEventSchema>,
    IProjectionHandler<BroadcastScheduledEventSchema>,
    IProjectionHandler<BroadcastStartedEventSchema>,
    IProjectionHandler<BroadcastPausedEventSchema>,
    IProjectionHandler<BroadcastResumedEventSchema>,
    IProjectionHandler<BroadcastEndedEventSchema>,
    IProjectionHandler<BroadcastCancelledEventSchema>
{
    private readonly PostgresProjectionStore<BroadcastReadModel> _store;

    public BroadcastProjectionHandler(PostgresProjectionStore<BroadcastReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            BroadcastCreatedEventSchema e => Project(e.AggregateId, s => BroadcastProjectionReducer.Apply(s, e), "BroadcastCreatedEvent", envelope, cancellationToken),
            BroadcastScheduledEventSchema e => Project(e.AggregateId, s => BroadcastProjectionReducer.Apply(s, e), "BroadcastScheduledEvent", envelope, cancellationToken),
            BroadcastStartedEventSchema e => Project(e.AggregateId, s => BroadcastProjectionReducer.Apply(s, e), "BroadcastStartedEvent", envelope, cancellationToken),
            BroadcastPausedEventSchema e => Project(e.AggregateId, s => BroadcastProjectionReducer.Apply(s, e), "BroadcastPausedEvent", envelope, cancellationToken),
            BroadcastResumedEventSchema e => Project(e.AggregateId, s => BroadcastProjectionReducer.Apply(s, e), "BroadcastResumedEvent", envelope, cancellationToken),
            BroadcastEndedEventSchema e => Project(e.AggregateId, s => BroadcastProjectionReducer.Apply(s, e), "BroadcastEndedEvent", envelope, cancellationToken),
            BroadcastCancelledEventSchema e => Project(e.AggregateId, s => BroadcastProjectionReducer.Apply(s, e), "BroadcastCancelledEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"BroadcastProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(BroadcastCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => BroadcastProjectionReducer.Apply(s, e), "BroadcastCreatedEvent", null, ct);
    public Task HandleAsync(BroadcastScheduledEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => BroadcastProjectionReducer.Apply(s, e), "BroadcastScheduledEvent", null, ct);
    public Task HandleAsync(BroadcastStartedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => BroadcastProjectionReducer.Apply(s, e), "BroadcastStartedEvent", null, ct);
    public Task HandleAsync(BroadcastPausedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => BroadcastProjectionReducer.Apply(s, e), "BroadcastPausedEvent", null, ct);
    public Task HandleAsync(BroadcastResumedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => BroadcastProjectionReducer.Apply(s, e), "BroadcastResumedEvent", null, ct);
    public Task HandleAsync(BroadcastEndedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => BroadcastProjectionReducer.Apply(s, e), "BroadcastEndedEvent", null, ct);
    public Task HandleAsync(BroadcastCancelledEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => BroadcastProjectionReducer.Apply(s, e), "BroadcastCancelledEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<BroadcastReadModel, BroadcastReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new BroadcastReadModel { BroadcastId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
