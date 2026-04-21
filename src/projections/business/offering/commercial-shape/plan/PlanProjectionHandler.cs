using Whycespace.Projections.Business.Offering.CommercialShape.Plan.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Plan;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Offering.CommercialShape.Plan;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Offering.CommercialShape.Plan;

public sealed class PlanProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<PlanDraftedEventSchema>,
    IProjectionHandler<PlanActivatedEventSchema>,
    IProjectionHandler<PlanDeprecatedEventSchema>,
    IProjectionHandler<PlanArchivedEventSchema>
{
    private readonly PostgresProjectionStore<PlanReadModel> _store;

    public PlanProjectionHandler(PostgresProjectionStore<PlanReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            PlanDraftedEventSchema e    => Project(e.AggregateId, s => PlanProjectionReducer.Apply(s, e), "PlanDraftedEvent",    envelope, cancellationToken),
            PlanActivatedEventSchema e  => Project(e.AggregateId, s => PlanProjectionReducer.Apply(s, e), "PlanActivatedEvent",  envelope, cancellationToken),
            PlanDeprecatedEventSchema e => Project(e.AggregateId, s => PlanProjectionReducer.Apply(s, e), "PlanDeprecatedEvent", envelope, cancellationToken),
            PlanArchivedEventSchema e   => Project(e.AggregateId, s => PlanProjectionReducer.Apply(s, e), "PlanArchivedEvent",   envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"PlanProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(PlanDraftedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PlanProjectionReducer.Apply(s, e), "PlanDraftedEvent", null, ct);

    public Task HandleAsync(PlanActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PlanProjectionReducer.Apply(s, e), "PlanActivatedEvent", null, ct);

    public Task HandleAsync(PlanDeprecatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PlanProjectionReducer.Apply(s, e), "PlanDeprecatedEvent", null, ct);

    public Task HandleAsync(PlanArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => PlanProjectionReducer.Apply(s, e), "PlanArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<PlanReadModel, PlanReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new PlanReadModel { PlanId = aggregateId };
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
