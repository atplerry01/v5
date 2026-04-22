using Whycespace.Projections.Control.SystemPolicy.PolicyEnforcement.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyEnforcement;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyEnforcement;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.SystemPolicy.PolicyEnforcement;

public sealed class PolicyEnforcementProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<PolicyEnforcedEventSchema>
{
    private readonly PostgresProjectionStore<PolicyEnforcementReadModel> _store;

    public PolicyEnforcementProjectionHandler(PostgresProjectionStore<PolicyEnforcementReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            PolicyEnforcedEventSchema e => Project(e.AggregateId, s => PolicyEnforcementProjectionReducer.Apply(s, e), "PolicyEnforcedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"PolicyEnforcementProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(PolicyEnforcedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PolicyEnforcementProjectionReducer.Apply(s, e), "PolicyEnforcedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<PolicyEnforcementReadModel, PolicyEnforcementReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new PolicyEnforcementReadModel { EnforcementId = aggregateId };
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
