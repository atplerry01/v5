using Whycespace.Projections.Platform.Routing.RouteResolution.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Platform.Routing.RouteResolution;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Platform.Routing.RouteResolution;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Platform.Routing.RouteResolution;

public sealed class RouteResolutionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<RouteResolvedEventSchema>,
    IProjectionHandler<RouteResolutionFailedEventSchema>
{
    private readonly PostgresProjectionStore<RouteResolutionReadModel> _store;

    public RouteResolutionProjectionHandler(PostgresProjectionStore<RouteResolutionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            RouteResolvedEventSchema e => Project(e.AggregateId, s => RouteResolutionProjectionReducer.Apply(s, e, envelope.Timestamp), "RouteResolvedEvent", envelope, cancellationToken),
            RouteResolutionFailedEventSchema e => Project(e.AggregateId, s => RouteResolutionProjectionReducer.Apply(s, e, envelope.Timestamp), "RouteResolutionFailedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException($"RouteResolutionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(RouteResolvedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => RouteResolutionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "RouteResolvedEvent", null, ct);
    public Task HandleAsync(RouteResolutionFailedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => RouteResolutionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "RouteResolutionFailedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<RouteResolutionReadModel, RouteResolutionReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new RouteResolutionReadModel { ResolutionId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
