using Whycespace.Projections.Shared;
using Whycespace.Projections.Trust.Access.Session.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Trust.Access.Session;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Trust.Access.Session;

namespace Whycespace.Projections.Trust.Access.Session;

public sealed class SessionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<SessionOpenedEventSchema>,
    IProjectionHandler<SessionExpiredEventSchema>,
    IProjectionHandler<SessionTerminatedEventSchema>
{
    private readonly PostgresProjectionStore<SessionReadModel> _store;

    public SessionProjectionHandler(PostgresProjectionStore<SessionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            SessionOpenedEventSchema e => Project(e.AggregateId, s => SessionProjectionReducer.Apply(s, e), "SessionOpenedEvent", envelope, cancellationToken),
            SessionExpiredEventSchema e => Project(e.AggregateId, s => SessionProjectionReducer.Apply(s, e), "SessionExpiredEvent", envelope, cancellationToken),
            SessionTerminatedEventSchema e => Project(e.AggregateId, s => SessionProjectionReducer.Apply(s, e), "SessionTerminatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"SessionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(SessionOpenedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SessionProjectionReducer.Apply(s, e), "SessionOpenedEvent", null, ct);

    public Task HandleAsync(SessionExpiredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SessionProjectionReducer.Apply(s, e), "SessionExpiredEvent", null, ct);

    public Task HandleAsync(SessionTerminatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SessionProjectionReducer.Apply(s, e), "SessionTerminatedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<SessionReadModel, SessionReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new SessionReadModel { SessionId = aggregateId };
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
