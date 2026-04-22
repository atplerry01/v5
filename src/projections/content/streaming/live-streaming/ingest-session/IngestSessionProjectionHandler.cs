using Whycespace.Projections.Content.Streaming.LiveStreaming.IngestSession.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.IngestSession;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Streaming.LiveStreaming.IngestSession;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Streaming.LiveStreaming.IngestSession;

public sealed class IngestSessionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<IngestSessionAuthenticatedEventSchema>,
    IProjectionHandler<IngestStreamingStartedEventSchema>,
    IProjectionHandler<IngestSessionStalledEventSchema>,
    IProjectionHandler<IngestSessionResumedEventSchema>,
    IProjectionHandler<IngestSessionEndedEventSchema>,
    IProjectionHandler<IngestSessionFailedEventSchema>
{
    private readonly PostgresProjectionStore<IngestSessionReadModel> _store;

    public IngestSessionProjectionHandler(PostgresProjectionStore<IngestSessionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            IngestSessionAuthenticatedEventSchema e => Project(e.AggregateId, s => IngestSessionProjectionReducer.Apply(s, e), "IngestSessionAuthenticatedEvent", envelope, cancellationToken),
            IngestStreamingStartedEventSchema e => Project(e.AggregateId, s => IngestSessionProjectionReducer.Apply(s, e), "IngestStreamingStartedEvent", envelope, cancellationToken),
            IngestSessionStalledEventSchema e => Project(e.AggregateId, s => IngestSessionProjectionReducer.Apply(s, e), "IngestSessionStalledEvent", envelope, cancellationToken),
            IngestSessionResumedEventSchema e => Project(e.AggregateId, s => IngestSessionProjectionReducer.Apply(s, e), "IngestSessionResumedEvent", envelope, cancellationToken),
            IngestSessionEndedEventSchema e => Project(e.AggregateId, s => IngestSessionProjectionReducer.Apply(s, e), "IngestSessionEndedEvent", envelope, cancellationToken),
            IngestSessionFailedEventSchema e => Project(e.AggregateId, s => IngestSessionProjectionReducer.Apply(s, e), "IngestSessionFailedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"IngestSessionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(IngestSessionAuthenticatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => IngestSessionProjectionReducer.Apply(s, e), "IngestSessionAuthenticatedEvent", null, ct);
    public Task HandleAsync(IngestStreamingStartedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => IngestSessionProjectionReducer.Apply(s, e), "IngestStreamingStartedEvent", null, ct);
    public Task HandleAsync(IngestSessionStalledEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => IngestSessionProjectionReducer.Apply(s, e), "IngestSessionStalledEvent", null, ct);
    public Task HandleAsync(IngestSessionResumedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => IngestSessionProjectionReducer.Apply(s, e), "IngestSessionResumedEvent", null, ct);
    public Task HandleAsync(IngestSessionEndedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => IngestSessionProjectionReducer.Apply(s, e), "IngestSessionEndedEvent", null, ct);
    public Task HandleAsync(IngestSessionFailedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => IngestSessionProjectionReducer.Apply(s, e), "IngestSessionFailedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<IngestSessionReadModel, IngestSessionReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new IngestSessionReadModel { SessionId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
