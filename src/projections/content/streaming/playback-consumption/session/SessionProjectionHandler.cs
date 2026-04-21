using Whycespace.Projections.Content.Streaming.PlaybackConsumption.Session.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Session;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Streaming.PlaybackConsumption.Session;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Streaming.PlaybackConsumption.Session;

public sealed class SessionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<SessionOpenedEventSchema>,
    IProjectionHandler<SessionActivatedEventSchema>,
    IProjectionHandler<SessionSuspendedEventSchema>,
    IProjectionHandler<SessionResumedEventSchema>,
    IProjectionHandler<SessionClosedEventSchema>,
    IProjectionHandler<SessionFailedEventSchema>,
    IProjectionHandler<SessionExpiredEventSchema>
{
    private readonly PostgresProjectionStore<SessionReadModel> _store;

    public SessionProjectionHandler(PostgresProjectionStore<SessionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            SessionOpenedEventSchema e => Project(e.AggregateId, s => SessionProjectionReducer.Apply(s, e), "SessionOpenedEvent", envelope, cancellationToken),
            SessionActivatedEventSchema e => Project(e.AggregateId, s => SessionProjectionReducer.Apply(s, e), "SessionActivatedEvent", envelope, cancellationToken),
            SessionSuspendedEventSchema e => Project(e.AggregateId, s => SessionProjectionReducer.Apply(s, e), "SessionSuspendedEvent", envelope, cancellationToken),
            SessionResumedEventSchema e => Project(e.AggregateId, s => SessionProjectionReducer.Apply(s, e), "SessionResumedEvent", envelope, cancellationToken),
            SessionClosedEventSchema e => Project(e.AggregateId, s => SessionProjectionReducer.Apply(s, e), "SessionClosedEvent", envelope, cancellationToken),
            SessionFailedEventSchema e => Project(e.AggregateId, s => SessionProjectionReducer.Apply(s, e), "SessionFailedEvent", envelope, cancellationToken),
            SessionExpiredEventSchema e => Project(e.AggregateId, s => SessionProjectionReducer.Apply(s, e), "SessionExpiredEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"SessionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(SessionOpenedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SessionProjectionReducer.Apply(s, e), "SessionOpenedEvent", null, ct);
    public Task HandleAsync(SessionActivatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SessionProjectionReducer.Apply(s, e), "SessionActivatedEvent", null, ct);
    public Task HandleAsync(SessionSuspendedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SessionProjectionReducer.Apply(s, e), "SessionSuspendedEvent", null, ct);
    public Task HandleAsync(SessionResumedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SessionProjectionReducer.Apply(s, e), "SessionResumedEvent", null, ct);
    public Task HandleAsync(SessionClosedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SessionProjectionReducer.Apply(s, e), "SessionClosedEvent", null, ct);
    public Task HandleAsync(SessionFailedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SessionProjectionReducer.Apply(s, e), "SessionFailedEvent", null, ct);
    public Task HandleAsync(SessionExpiredEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SessionProjectionReducer.Apply(s, e), "SessionExpiredEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<SessionReadModel, SessionReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new SessionReadModel { SessionId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
