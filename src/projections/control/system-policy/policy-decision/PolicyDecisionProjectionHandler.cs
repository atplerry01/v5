using Whycespace.Projections.Control.SystemPolicy.PolicyDecision.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyDecision;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyDecision;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.SystemPolicy.PolicyDecision;

public sealed class PolicyDecisionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<PolicyDecisionRecordedEventSchema>
{
    private readonly PostgresProjectionStore<PolicyDecisionReadModel> _store;

    public PolicyDecisionProjectionHandler(PostgresProjectionStore<PolicyDecisionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            PolicyDecisionRecordedEventSchema e => Project(e.AggregateId, s => PolicyDecisionProjectionReducer.Apply(s, e), "PolicyDecisionRecordedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"PolicyDecisionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(PolicyDecisionRecordedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PolicyDecisionProjectionReducer.Apply(s, e), "PolicyDecisionRecordedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<PolicyDecisionReadModel, PolicyDecisionReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new PolicyDecisionReadModel { DecisionId = aggregateId };
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
