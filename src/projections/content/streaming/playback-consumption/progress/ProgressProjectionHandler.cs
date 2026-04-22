using Whycespace.Projections.Content.Streaming.PlaybackConsumption.Progress.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Progress;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Streaming.PlaybackConsumption.Progress;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Streaming.PlaybackConsumption.Progress;

public sealed class ProgressProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ProgressTrackedEventSchema>,
    IProjectionHandler<PlaybackPositionUpdatedEventSchema>,
    IProjectionHandler<PlaybackPausedEventSchema>,
    IProjectionHandler<PlaybackResumedEventSchema>,
    IProjectionHandler<ProgressTerminatedEventSchema>
{
    private readonly PostgresProjectionStore<ProgressReadModel> _store;

    public ProgressProjectionHandler(PostgresProjectionStore<ProgressReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            ProgressTrackedEventSchema e => Project(e.AggregateId, s => ProgressProjectionReducer.Apply(s, e), "ProgressTrackedEvent", envelope, cancellationToken),
            PlaybackPositionUpdatedEventSchema e => Project(e.AggregateId, s => ProgressProjectionReducer.Apply(s, e), "PlaybackPositionUpdatedEvent", envelope, cancellationToken),
            PlaybackPausedEventSchema e => Project(e.AggregateId, s => ProgressProjectionReducer.Apply(s, e), "PlaybackPausedEvent", envelope, cancellationToken),
            PlaybackResumedEventSchema e => Project(e.AggregateId, s => ProgressProjectionReducer.Apply(s, e), "PlaybackResumedEvent", envelope, cancellationToken),
            ProgressTerminatedEventSchema e => Project(e.AggregateId, s => ProgressProjectionReducer.Apply(s, e), "ProgressTerminatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ProgressProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(ProgressTrackedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ProgressProjectionReducer.Apply(s, e), "ProgressTrackedEvent", null, ct);
    public Task HandleAsync(PlaybackPositionUpdatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ProgressProjectionReducer.Apply(s, e), "PlaybackPositionUpdatedEvent", null, ct);
    public Task HandleAsync(PlaybackPausedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ProgressProjectionReducer.Apply(s, e), "PlaybackPausedEvent", null, ct);
    public Task HandleAsync(PlaybackResumedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ProgressProjectionReducer.Apply(s, e), "PlaybackResumedEvent", null, ct);
    public Task HandleAsync(ProgressTerminatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ProgressProjectionReducer.Apply(s, e), "ProgressTerminatedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<ProgressReadModel, ProgressReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new ProgressReadModel { ProgressId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
