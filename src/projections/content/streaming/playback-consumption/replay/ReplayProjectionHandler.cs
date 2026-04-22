using Whycespace.Projections.Content.Streaming.PlaybackConsumption.Replay.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Replay;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Streaming.PlaybackConsumption.Replay;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Streaming.PlaybackConsumption.Replay;

public sealed class ReplayProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ReplayRequestedEventSchema>,
    IProjectionHandler<ReplayStartedEventSchema>,
    IProjectionHandler<ReplayPausedEventSchema>,
    IProjectionHandler<ReplayResumedEventSchema>,
    IProjectionHandler<ReplayCompletedEventSchema>,
    IProjectionHandler<ReplayAbandonedEventSchema>
{
    private readonly PostgresProjectionStore<ReplayReadModel> _store;

    public ReplayProjectionHandler(PostgresProjectionStore<ReplayReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            ReplayRequestedEventSchema e => Project(e.AggregateId, s => ReplayProjectionReducer.Apply(s, e), "ReplayRequestedEvent", envelope, cancellationToken),
            ReplayStartedEventSchema e => Project(e.AggregateId, s => ReplayProjectionReducer.Apply(s, e), "ReplayStartedEvent", envelope, cancellationToken),
            ReplayPausedEventSchema e => Project(e.AggregateId, s => ReplayProjectionReducer.Apply(s, e), "ReplayPausedEvent", envelope, cancellationToken),
            ReplayResumedEventSchema e => Project(e.AggregateId, s => ReplayProjectionReducer.Apply(s, e), "ReplayResumedEvent", envelope, cancellationToken),
            ReplayCompletedEventSchema e => Project(e.AggregateId, s => ReplayProjectionReducer.Apply(s, e), "ReplayCompletedEvent", envelope, cancellationToken),
            ReplayAbandonedEventSchema e => Project(e.AggregateId, s => ReplayProjectionReducer.Apply(s, e), "ReplayAbandonedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ReplayProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(ReplayRequestedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ReplayProjectionReducer.Apply(s, e), "ReplayRequestedEvent", null, ct);
    public Task HandleAsync(ReplayStartedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ReplayProjectionReducer.Apply(s, e), "ReplayStartedEvent", null, ct);
    public Task HandleAsync(ReplayPausedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ReplayProjectionReducer.Apply(s, e), "ReplayPausedEvent", null, ct);
    public Task HandleAsync(ReplayResumedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ReplayProjectionReducer.Apply(s, e), "ReplayResumedEvent", null, ct);
    public Task HandleAsync(ReplayCompletedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ReplayProjectionReducer.Apply(s, e), "ReplayCompletedEvent", null, ct);
    public Task HandleAsync(ReplayAbandonedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ReplayProjectionReducer.Apply(s, e), "ReplayAbandonedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<ReplayReadModel, ReplayReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new ReplayReadModel { ReplayId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
