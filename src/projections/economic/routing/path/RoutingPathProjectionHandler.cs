using Whycespace.Projections.Economic.Routing.Path.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Routing.Path;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Routing.Path;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Routing.Path;

public sealed class RoutingPathProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<RoutingPathDefinedEventSchema>,
    IProjectionHandler<RoutingPathActivatedEventSchema>,
    IProjectionHandler<RoutingPathDisabledEventSchema>
{
    private readonly PostgresProjectionStore<RoutingPathReadModel> _store;

    public RoutingPathProjectionHandler(PostgresProjectionStore<RoutingPathReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            RoutingPathDefinedEventSchema e   => Project(e.AggregateId, s => RoutingPathProjectionReducer.Apply(s, e), "RoutingPathDefinedEvent", envelope, cancellationToken),
            RoutingPathActivatedEventSchema e => Project(e.AggregateId, s => RoutingPathProjectionReducer.Apply(s, e), "RoutingPathActivatedEvent", envelope, cancellationToken),
            RoutingPathDisabledEventSchema e  => Project(e.AggregateId, s => RoutingPathProjectionReducer.Apply(s, e), "RoutingPathDisabledEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"RoutingPathProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(RoutingPathDefinedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RoutingPathProjectionReducer.Apply(s, e), "RoutingPathDefinedEvent", null, ct);

    public Task HandleAsync(RoutingPathActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RoutingPathProjectionReducer.Apply(s, e), "RoutingPathActivatedEvent", null, ct);

    public Task HandleAsync(RoutingPathDisabledEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RoutingPathProjectionReducer.Apply(s, e), "RoutingPathDisabledEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<RoutingPathReadModel, RoutingPathReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new RoutingPathReadModel { PathId = aggregateId };
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
