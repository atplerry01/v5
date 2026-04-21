using Whycespace.Projections.Business.Entitlement.EligibilityAndGrant.Assignment.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Assignment;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Entitlement.EligibilityAndGrant.Assignment;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Entitlement.EligibilityAndGrant.Assignment;

public sealed class AssignmentProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<AssignmentCreatedEventSchema>,
    IProjectionHandler<AssignmentActivatedEventSchema>,
    IProjectionHandler<AssignmentRevokedEventSchema>
{
    private readonly PostgresProjectionStore<AssignmentReadModel> _store;

    public AssignmentProjectionHandler(PostgresProjectionStore<AssignmentReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            AssignmentCreatedEventSchema e   => Project(e.AggregateId, s => AssignmentProjectionReducer.Apply(s, e), "AssignmentCreatedEvent",   envelope, cancellationToken),
            AssignmentActivatedEventSchema e => Project(e.AggregateId, s => AssignmentProjectionReducer.Apply(s, e), "AssignmentActivatedEvent", envelope, cancellationToken),
            AssignmentRevokedEventSchema e   => Project(e.AggregateId, s => AssignmentProjectionReducer.Apply(s, e), "AssignmentRevokedEvent",   envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"AssignmentProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(AssignmentCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AssignmentProjectionReducer.Apply(s, e), "AssignmentCreatedEvent", null, ct);

    public Task HandleAsync(AssignmentActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AssignmentProjectionReducer.Apply(s, e), "AssignmentActivatedEvent", null, ct);

    public Task HandleAsync(AssignmentRevokedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AssignmentProjectionReducer.Apply(s, e), "AssignmentRevokedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<AssignmentReadModel, AssignmentReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new AssignmentReadModel { AssignmentId = aggregateId };
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
