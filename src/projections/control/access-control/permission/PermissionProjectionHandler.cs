using Whycespace.Projections.Control.AccessControl.Permission.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.AccessControl.Permission;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.AccessControl.Permission;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.AccessControl.Permission;

public sealed class PermissionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<PermissionDefinedEventSchema>,
    IProjectionHandler<PermissionDeprecatedEventSchema>
{
    private readonly PostgresProjectionStore<PermissionReadModel> _store;

    public PermissionProjectionHandler(PostgresProjectionStore<PermissionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            PermissionDefinedEventSchema e    => Project(e.AggregateId, s => PermissionProjectionReducer.Apply(s, e), "PermissionDefinedEvent",    envelope, cancellationToken),
            PermissionDeprecatedEventSchema e => Project(e.AggregateId, s => PermissionProjectionReducer.Apply(s, e), "PermissionDeprecatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"PermissionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(PermissionDefinedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PermissionProjectionReducer.Apply(s, e), "PermissionDefinedEvent", null, ct);

    public Task HandleAsync(PermissionDeprecatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PermissionProjectionReducer.Apply(s, e), "PermissionDeprecatedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<PermissionReadModel, PermissionReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new PermissionReadModel { PermissionId = aggregateId };
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
