using Whycespace.Projections.Control.AccessControl.Identity.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.AccessControl.Identity;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.AccessControl.Identity;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.AccessControl.Identity;

public sealed class IdentityProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<IdentityRegisteredEventSchema>,
    IProjectionHandler<IdentitySuspendedEventSchema>,
    IProjectionHandler<IdentityDeactivatedEventSchema>
{
    private readonly PostgresProjectionStore<IdentityReadModel> _store;

    public IdentityProjectionHandler(PostgresProjectionStore<IdentityReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            IdentityRegisteredEventSchema e   => Project(e.AggregateId, s => IdentityProjectionReducer.Apply(s, e), "IdentityRegisteredEvent",   envelope, cancellationToken),
            IdentitySuspendedEventSchema e    => Project(e.AggregateId, s => IdentityProjectionReducer.Apply(s, e), "IdentitySuspendedEvent",    envelope, cancellationToken),
            IdentityDeactivatedEventSchema e  => Project(e.AggregateId, s => IdentityProjectionReducer.Apply(s, e), "IdentityDeactivatedEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"IdentityProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(IdentityRegisteredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => IdentityProjectionReducer.Apply(s, e), "IdentityRegisteredEvent", null, ct);

    public Task HandleAsync(IdentitySuspendedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => IdentityProjectionReducer.Apply(s, e), "IdentitySuspendedEvent", null, ct);

    public Task HandleAsync(IdentityDeactivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => IdentityProjectionReducer.Apply(s, e), "IdentityDeactivatedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<IdentityReadModel, IdentityReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new IdentityReadModel { IdentityId = aggregateId };
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
