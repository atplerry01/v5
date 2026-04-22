using Whycespace.Projections.Platform.Routing.RouteDefinition.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Platform.Routing.RouteDefinition;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Platform.Routing.RouteDefinition;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Platform.Routing.RouteDefinition;

public sealed class RouteDefinitionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<RouteDefinitionRegisteredEventSchema>,
    IProjectionHandler<RouteDefinitionDeactivatedEventSchema>,
    IProjectionHandler<RouteDefinitionDeprecatedEventSchema>
{
    private readonly PostgresProjectionStore<RouteDefinitionReadModel> _store;

    public RouteDefinitionProjectionHandler(PostgresProjectionStore<RouteDefinitionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            RouteDefinitionRegisteredEventSchema e => Project(e.AggregateId, s => RouteDefinitionProjectionReducer.Apply(s, e, envelope.Timestamp), "RouteDefinitionRegisteredEvent", envelope, cancellationToken),
            RouteDefinitionDeactivatedEventSchema e => Project(e.AggregateId, s => RouteDefinitionProjectionReducer.Apply(s, e, envelope.Timestamp), "RouteDefinitionDeactivatedEvent", envelope, cancellationToken),
            RouteDefinitionDeprecatedEventSchema e => Project(e.AggregateId, s => RouteDefinitionProjectionReducer.Apply(s, e, envelope.Timestamp), "RouteDefinitionDeprecatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException($"RouteDefinitionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(RouteDefinitionRegisteredEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => RouteDefinitionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "RouteDefinitionRegisteredEvent", null, ct);
    public Task HandleAsync(RouteDefinitionDeactivatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => RouteDefinitionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "RouteDefinitionDeactivatedEvent", null, ct);
    public Task HandleAsync(RouteDefinitionDeprecatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => RouteDefinitionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "RouteDefinitionDeprecatedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<RouteDefinitionReadModel, RouteDefinitionReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new RouteDefinitionReadModel { RouteDefinitionId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
