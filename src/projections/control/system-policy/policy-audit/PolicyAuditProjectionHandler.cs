using Whycespace.Projections.Control.SystemPolicy.PolicyAudit.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyAudit;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyAudit;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.SystemPolicy.PolicyAudit;

public sealed class PolicyAuditProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<PolicyAuditEntryRecordedEventSchema>,
    IProjectionHandler<PolicyAuditEntryReviewedEventSchema>
{
    private readonly PostgresProjectionStore<PolicyAuditReadModel> _store;

    public PolicyAuditProjectionHandler(PostgresProjectionStore<PolicyAuditReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            PolicyAuditEntryRecordedEventSchema e  => Project(e.AggregateId, s => PolicyAuditProjectionReducer.Apply(s, e), "PolicyAuditEntryRecordedEvent",  envelope, cancellationToken),
            PolicyAuditEntryReviewedEventSchema e  => Project(e.AggregateId, s => PolicyAuditProjectionReducer.Apply(s, e), "PolicyAuditEntryReviewedEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"PolicyAuditProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(PolicyAuditEntryRecordedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PolicyAuditProjectionReducer.Apply(s, e), "PolicyAuditEntryRecordedEvent", null, ct);

    public Task HandleAsync(PolicyAuditEntryReviewedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PolicyAuditProjectionReducer.Apply(s, e), "PolicyAuditEntryReviewedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<PolicyAuditReadModel, PolicyAuditReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new PolicyAuditReadModel { AuditId = aggregateId };
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
