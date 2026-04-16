using Whycespace.Projections.Economic.Enforcement.Restriction.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Enforcement.Restriction;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Enforcement.Restriction;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Enforcement.Restriction;

public sealed class RestrictionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<RestrictionAppliedEventSchema>,
    IProjectionHandler<RestrictionUpdatedEventSchema>,
    IProjectionHandler<RestrictionRemovedEventSchema>
{
    private readonly PostgresProjectionStore<RestrictionReadModel> _store;

    public RestrictionProjectionHandler(PostgresProjectionStore<RestrictionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            RestrictionAppliedEventSchema e => Project(e.AggregateId, s => RestrictionProjectionReducer.Apply(s, e), "RestrictionAppliedEvent", envelope, cancellationToken),
            RestrictionUpdatedEventSchema e => Project(e.AggregateId, s => RestrictionProjectionReducer.Apply(s, e), "RestrictionUpdatedEvent", envelope, cancellationToken),
            RestrictionRemovedEventSchema e => Project(e.AggregateId, s => RestrictionProjectionReducer.Apply(s, e), "RestrictionRemovedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"RestrictionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(RestrictionAppliedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RestrictionProjectionReducer.Apply(s, e), "RestrictionAppliedEvent", null, ct);

    public Task HandleAsync(RestrictionUpdatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RestrictionProjectionReducer.Apply(s, e), "RestrictionUpdatedEvent", null, ct);

    public Task HandleAsync(RestrictionRemovedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RestrictionProjectionReducer.Apply(s, e), "RestrictionRemovedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<RestrictionReadModel, RestrictionReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new RestrictionReadModel { RestrictionId = aggregateId };
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
