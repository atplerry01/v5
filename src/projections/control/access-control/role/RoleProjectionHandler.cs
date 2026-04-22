using Whycespace.Projections.Control.AccessControl.Role.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.AccessControl.Role;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.AccessControl.Role;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.AccessControl.Role;

public sealed class RoleProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<RoleDefinedEventSchema>,
    IProjectionHandler<RolePermissionAddedEventSchema>,
    IProjectionHandler<RoleDeprecatedEventSchema>
{
    private readonly PostgresProjectionStore<RoleReadModel> _store;

    public RoleProjectionHandler(PostgresProjectionStore<RoleReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            RoleDefinedEventSchema e         => Project(e.AggregateId, s => RoleProjectionReducer.Apply(s, e), "RoleDefinedEvent",         envelope, cancellationToken),
            RolePermissionAddedEventSchema e => Project(e.AggregateId, s => RoleProjectionReducer.Apply(s, e), "RolePermissionAddedEvent", envelope, cancellationToken),
            RoleDeprecatedEventSchema e      => Project(e.AggregateId, s => RoleProjectionReducer.Apply(s, e), "RoleDeprecatedEvent",      envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"RoleProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(RoleDefinedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RoleProjectionReducer.Apply(s, e), "RoleDefinedEvent", null, ct);

    public Task HandleAsync(RolePermissionAddedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RoleProjectionReducer.Apply(s, e), "RolePermissionAddedEvent", null, ct);

    public Task HandleAsync(RoleDeprecatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RoleProjectionReducer.Apply(s, e), "RoleDeprecatedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<RoleReadModel, RoleReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new RoleReadModel { RoleId = aggregateId };
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
