using Whycespace.Projections.Business.Service.ServiceConstraint.PolicyBinding.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.PolicyBinding;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Service.ServiceConstraint.PolicyBinding;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Service.ServiceConstraint.PolicyBinding;

public sealed class PolicyBindingProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<PolicyBindingCreatedEventSchema>,
    IProjectionHandler<PolicyBindingBoundEventSchema>,
    IProjectionHandler<PolicyBindingUnboundEventSchema>,
    IProjectionHandler<PolicyBindingArchivedEventSchema>
{
    private readonly PostgresProjectionStore<PolicyBindingReadModel> _store;

    public PolicyBindingProjectionHandler(PostgresProjectionStore<PolicyBindingReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            PolicyBindingCreatedEventSchema e  => Project(e.AggregateId, s => PolicyBindingProjectionReducer.Apply(s, e), "PolicyBindingCreatedEvent",  envelope, cancellationToken),
            PolicyBindingBoundEventSchema e    => Project(e.AggregateId, s => PolicyBindingProjectionReducer.Apply(s, e), "PolicyBindingBoundEvent",    envelope, cancellationToken),
            PolicyBindingUnboundEventSchema e  => Project(e.AggregateId, s => PolicyBindingProjectionReducer.Apply(s, e), "PolicyBindingUnboundEvent",  envelope, cancellationToken),
            PolicyBindingArchivedEventSchema e => Project(e.AggregateId, s => PolicyBindingProjectionReducer.Apply(s, e), "PolicyBindingArchivedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"PolicyBindingProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(PolicyBindingCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PolicyBindingProjectionReducer.Apply(s, e), "PolicyBindingCreatedEvent", null, ct);

    public Task HandleAsync(PolicyBindingBoundEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PolicyBindingProjectionReducer.Apply(s, e), "PolicyBindingBoundEvent", null, ct);

    public Task HandleAsync(PolicyBindingUnboundEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PolicyBindingProjectionReducer.Apply(s, e), "PolicyBindingUnboundEvent", null, ct);

    public Task HandleAsync(PolicyBindingArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PolicyBindingProjectionReducer.Apply(s, e), "PolicyBindingArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<PolicyBindingReadModel, PolicyBindingReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new PolicyBindingReadModel { PolicyBindingId = aggregateId };
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
