using Whycespace.Projections.Business.Entitlement.EligibilityAndGrant.Eligibility.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Eligibility;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Entitlement.EligibilityAndGrant.Eligibility;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Entitlement.EligibilityAndGrant.Eligibility;

public sealed class EligibilityProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<EligibilityCreatedEventSchema>,
    IProjectionHandler<EligibilityEvaluatedEligibleEventSchema>,
    IProjectionHandler<EligibilityEvaluatedIneligibleEventSchema>
{
    private readonly PostgresProjectionStore<EligibilityReadModel> _store;

    public EligibilityProjectionHandler(PostgresProjectionStore<EligibilityReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            EligibilityCreatedEventSchema e              => Project(e.AggregateId, s => EligibilityProjectionReducer.Apply(s, e), "EligibilityCreatedEvent",              envelope, cancellationToken),
            EligibilityEvaluatedEligibleEventSchema e    => Project(e.AggregateId, s => EligibilityProjectionReducer.Apply(s, e), "EligibilityEvaluatedEligibleEvent",    envelope, cancellationToken),
            EligibilityEvaluatedIneligibleEventSchema e  => Project(e.AggregateId, s => EligibilityProjectionReducer.Apply(s, e), "EligibilityEvaluatedIneligibleEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"EligibilityProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(EligibilityCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => EligibilityProjectionReducer.Apply(s, e), "EligibilityCreatedEvent", null, ct);

    public Task HandleAsync(EligibilityEvaluatedEligibleEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => EligibilityProjectionReducer.Apply(s, e), "EligibilityEvaluatedEligibleEvent", null, ct);

    public Task HandleAsync(EligibilityEvaluatedIneligibleEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => EligibilityProjectionReducer.Apply(s, e), "EligibilityEvaluatedIneligibleEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<EligibilityReadModel, EligibilityReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new EligibilityReadModel { EligibilityId = aggregateId };
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
