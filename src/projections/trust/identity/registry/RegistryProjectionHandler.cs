using Whycespace.Projections.Shared;
using Whycespace.Projections.Trust.Identity.Registry.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Trust.Identity.Registry;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Trust.Identity.Registry;

namespace Whycespace.Projections.Trust.Identity.Registry;

public sealed class RegistryProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<RegistrationInitiatedEventSchema>,
    IProjectionHandler<RegistrationVerifiedEventSchema>,
    IProjectionHandler<RegistrationActivatedEventSchema>,
    IProjectionHandler<RegistrationRejectedEventSchema>,
    IProjectionHandler<RegistrationLockedEventSchema>
{
    private readonly PostgresProjectionStore<RegistryReadModel> _store;

    public RegistryProjectionHandler(PostgresProjectionStore<RegistryReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            RegistrationInitiatedEventSchema e => Project(e.AggregateId, s => RegistryProjectionReducer.Apply(s, e), "RegistrationInitiatedEvent", envelope, cancellationToken),
            RegistrationVerifiedEventSchema e => Project(e.AggregateId, s => RegistryProjectionReducer.Apply(s, e), "RegistrationVerifiedEvent", envelope, cancellationToken),
            RegistrationActivatedEventSchema e => Project(e.AggregateId, s => RegistryProjectionReducer.Apply(s, e), "RegistrationActivatedEvent", envelope, cancellationToken),
            RegistrationRejectedEventSchema e => Project(e.AggregateId, s => RegistryProjectionReducer.Apply(s, e), "RegistrationRejectedEvent", envelope, cancellationToken),
            RegistrationLockedEventSchema e => Project(e.AggregateId, s => RegistryProjectionReducer.Apply(s, e), "RegistrationLockedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"RegistryProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(RegistrationInitiatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RegistryProjectionReducer.Apply(s, e), "RegistrationInitiatedEvent", null, ct);

    public Task HandleAsync(RegistrationVerifiedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RegistryProjectionReducer.Apply(s, e), "RegistrationVerifiedEvent", null, ct);

    public Task HandleAsync(RegistrationActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RegistryProjectionReducer.Apply(s, e), "RegistrationActivatedEvent", null, ct);

    public Task HandleAsync(RegistrationRejectedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RegistryProjectionReducer.Apply(s, e), "RegistrationRejectedEvent", null, ct);

    public Task HandleAsync(RegistrationLockedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RegistryProjectionReducer.Apply(s, e), "RegistrationLockedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<RegistryReadModel, RegistryReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new RegistryReadModel { RegistryId = aggregateId };
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
