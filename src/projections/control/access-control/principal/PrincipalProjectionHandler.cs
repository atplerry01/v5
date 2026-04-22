using Whycespace.Projections.Control.AccessControl.Principal.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.AccessControl.Principal;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.AccessControl.Principal;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.AccessControl.Principal;

public sealed class PrincipalProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<PrincipalRegisteredEventSchema>,
    IProjectionHandler<PrincipalRoleAssignedEventSchema>,
    IProjectionHandler<PrincipalDeactivatedEventSchema>
{
    private readonly PostgresProjectionStore<PrincipalReadModel> _store;

    public PrincipalProjectionHandler(PostgresProjectionStore<PrincipalReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            PrincipalRegisteredEventSchema e  => Project(e.AggregateId, s => PrincipalProjectionReducer.Apply(s, e), "PrincipalRegisteredEvent",  envelope, cancellationToken),
            PrincipalRoleAssignedEventSchema e => Project(e.AggregateId, s => PrincipalProjectionReducer.Apply(s, e), "PrincipalRoleAssignedEvent", envelope, cancellationToken),
            PrincipalDeactivatedEventSchema e => Project(e.AggregateId, s => PrincipalProjectionReducer.Apply(s, e), "PrincipalDeactivatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"PrincipalProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(PrincipalRegisteredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PrincipalProjectionReducer.Apply(s, e), "PrincipalRegisteredEvent", null, ct);

    public Task HandleAsync(PrincipalRoleAssignedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PrincipalProjectionReducer.Apply(s, e), "PrincipalRoleAssignedEvent", null, ct);

    public Task HandleAsync(PrincipalDeactivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PrincipalProjectionReducer.Apply(s, e), "PrincipalDeactivatedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<PrincipalReadModel, PrincipalReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new PrincipalReadModel { PrincipalId = aggregateId };
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
