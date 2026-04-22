using Whycespace.Projections.Control.AccessControl.AccessPolicy.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.AccessControl.AccessPolicy;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.AccessControl.AccessPolicy;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.AccessControl.AccessPolicy;

public sealed class AccessPolicyProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<AccessPolicyDefinedEventSchema>,
    IProjectionHandler<AccessPolicyActivatedEventSchema>,
    IProjectionHandler<AccessPolicyRetiredEventSchema>
{
    private readonly PostgresProjectionStore<AccessPolicyReadModel> _store;

    public AccessPolicyProjectionHandler(PostgresProjectionStore<AccessPolicyReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            AccessPolicyDefinedEventSchema e   => Project(e.AggregateId, s => AccessPolicyProjectionReducer.Apply(s, e), "AccessPolicyDefinedEvent",   envelope, cancellationToken),
            AccessPolicyActivatedEventSchema e => Project(e.AggregateId, s => AccessPolicyProjectionReducer.Apply(s, e), "AccessPolicyActivatedEvent", envelope, cancellationToken),
            AccessPolicyRetiredEventSchema e   => Project(e.AggregateId, s => AccessPolicyProjectionReducer.Apply(s, e), "AccessPolicyRetiredEvent",   envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"AccessPolicyProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(AccessPolicyDefinedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AccessPolicyProjectionReducer.Apply(s, e), "AccessPolicyDefinedEvent", null, ct);

    public Task HandleAsync(AccessPolicyActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AccessPolicyProjectionReducer.Apply(s, e), "AccessPolicyActivatedEvent", null, ct);

    public Task HandleAsync(AccessPolicyRetiredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AccessPolicyProjectionReducer.Apply(s, e), "AccessPolicyRetiredEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<AccessPolicyReadModel, AccessPolicyReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new AccessPolicyReadModel { PolicyId = aggregateId };
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
