using Whycespace.Projections.Business.Agreement.ChangeControl.Clause.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Clause;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Agreement.ChangeControl.Clause;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Agreement.ChangeControl.Clause;

public sealed class ClauseProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ClauseCreatedEventSchema>,
    IProjectionHandler<ClauseActivatedEventSchema>,
    IProjectionHandler<ClauseSupersededEventSchema>
{
    private readonly PostgresProjectionStore<ClauseReadModel> _store;

    public ClauseProjectionHandler(PostgresProjectionStore<ClauseReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ClauseCreatedEventSchema e    => Project(e.AggregateId, s => ClauseProjectionReducer.Apply(s, e), "ClauseCreatedEvent",    envelope, cancellationToken),
            ClauseActivatedEventSchema e  => Project(e.AggregateId, s => ClauseProjectionReducer.Apply(s, e), "ClauseActivatedEvent",  envelope, cancellationToken),
            ClauseSupersededEventSchema e => Project(e.AggregateId, s => ClauseProjectionReducer.Apply(s, e), "ClauseSupersededEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ClauseProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ClauseCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ClauseProjectionReducer.Apply(s, e), "ClauseCreatedEvent", null, ct);

    public Task HandleAsync(ClauseActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ClauseProjectionReducer.Apply(s, e), "ClauseActivatedEvent", null, ct);

    public Task HandleAsync(ClauseSupersededEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ClauseProjectionReducer.Apply(s, e), "ClauseSupersededEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ClauseReadModel, ClauseReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ClauseReadModel { ClauseId = aggregateId };
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
