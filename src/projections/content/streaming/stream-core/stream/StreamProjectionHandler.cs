using Whycespace.Projections.Content.Streaming.StreamCore.Stream.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Stream;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Streaming.StreamCore.Stream;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Streaming.StreamCore.Stream;

public sealed class StreamProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<StreamCreatedEventSchema>,
    IProjectionHandler<StreamActivatedEventSchema>,
    IProjectionHandler<StreamPausedEventSchema>,
    IProjectionHandler<StreamResumedEventSchema>,
    IProjectionHandler<StreamEndedEventSchema>,
    IProjectionHandler<StreamArchivedEventSchema>
{
    private readonly PostgresProjectionStore<StreamReadModel> _store;

    public StreamProjectionHandler(PostgresProjectionStore<StreamReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            StreamCreatedEventSchema e => Project(e.AggregateId, s => StreamProjectionReducer.Apply(s, e), "StreamCreatedEvent", envelope, cancellationToken),
            StreamActivatedEventSchema e => Project(e.AggregateId, s => StreamProjectionReducer.Apply(s, e), "StreamActivatedEvent", envelope, cancellationToken),
            StreamPausedEventSchema e => Project(e.AggregateId, s => StreamProjectionReducer.Apply(s, e), "StreamPausedEvent", envelope, cancellationToken),
            StreamResumedEventSchema e => Project(e.AggregateId, s => StreamProjectionReducer.Apply(s, e), "StreamResumedEvent", envelope, cancellationToken),
            StreamEndedEventSchema e => Project(e.AggregateId, s => StreamProjectionReducer.Apply(s, e), "StreamEndedEvent", envelope, cancellationToken),
            StreamArchivedEventSchema e => Project(e.AggregateId, s => StreamProjectionReducer.Apply(s, e), "StreamArchivedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"StreamProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(StreamCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => StreamProjectionReducer.Apply(s, e), "StreamCreatedEvent", null, ct);
    public Task HandleAsync(StreamActivatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => StreamProjectionReducer.Apply(s, e), "StreamActivatedEvent", null, ct);
    public Task HandleAsync(StreamPausedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => StreamProjectionReducer.Apply(s, e), "StreamPausedEvent", null, ct);
    public Task HandleAsync(StreamResumedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => StreamProjectionReducer.Apply(s, e), "StreamResumedEvent", null, ct);
    public Task HandleAsync(StreamEndedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => StreamProjectionReducer.Apply(s, e), "StreamEndedEvent", null, ct);
    public Task HandleAsync(StreamArchivedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => StreamProjectionReducer.Apply(s, e), "StreamArchivedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<StreamReadModel, StreamReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new StreamReadModel { StreamId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
