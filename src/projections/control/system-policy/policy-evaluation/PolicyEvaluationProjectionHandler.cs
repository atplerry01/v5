using Whycespace.Projections.Control.SystemPolicy.PolicyEvaluation.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyEvaluation;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyEvaluation;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.SystemPolicy.PolicyEvaluation;

public sealed class PolicyEvaluationProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<PolicyEvaluationRecordedEventSchema>,
    IProjectionHandler<PolicyEvaluationVerdictIssuedEventSchema>
{
    private readonly PostgresProjectionStore<PolicyEvaluationReadModel> _store;

    public PolicyEvaluationProjectionHandler(PostgresProjectionStore<PolicyEvaluationReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            PolicyEvaluationRecordedEventSchema e      => Project(e.AggregateId, s => PolicyEvaluationProjectionReducer.Apply(s, e), "PolicyEvaluationRecordedEvent",      envelope, cancellationToken),
            PolicyEvaluationVerdictIssuedEventSchema e => Project(e.AggregateId, s => PolicyEvaluationProjectionReducer.Apply(s, e), "PolicyEvaluationVerdictIssuedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"PolicyEvaluationProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(PolicyEvaluationRecordedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PolicyEvaluationProjectionReducer.Apply(s, e), "PolicyEvaluationRecordedEvent", null, ct);

    public Task HandleAsync(PolicyEvaluationVerdictIssuedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PolicyEvaluationProjectionReducer.Apply(s, e), "PolicyEvaluationVerdictIssuedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<PolicyEvaluationReadModel, PolicyEvaluationReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new PolicyEvaluationReadModel { EvaluationId = aggregateId };
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
