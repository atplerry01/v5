using Whycespace.Projections.Business.Agreement.ChangeControl.Amendment.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Amendment;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Agreement.ChangeControl.Amendment;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Agreement.ChangeControl.Amendment;

public sealed class AmendmentProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<AmendmentCreatedEventSchema>,
    IProjectionHandler<AmendmentAppliedEventSchema>,
    IProjectionHandler<AmendmentRevertedEventSchema>
{
    private readonly PostgresProjectionStore<AmendmentReadModel> _store;

    public AmendmentProjectionHandler(PostgresProjectionStore<AmendmentReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            AmendmentCreatedEventSchema e  => Project(e.AggregateId, s => AmendmentProjectionReducer.Apply(s, e), "AmendmentCreatedEvent",  envelope, cancellationToken),
            AmendmentAppliedEventSchema e  => Project(e.AggregateId, s => AmendmentProjectionReducer.Apply(s, e), "AmendmentAppliedEvent",  envelope, cancellationToken),
            AmendmentRevertedEventSchema e => Project(e.AggregateId, s => AmendmentProjectionReducer.Apply(s, e), "AmendmentRevertedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"AmendmentProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(AmendmentCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AmendmentProjectionReducer.Apply(s, e), "AmendmentCreatedEvent", null, ct);

    public Task HandleAsync(AmendmentAppliedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AmendmentProjectionReducer.Apply(s, e), "AmendmentAppliedEvent", null, ct);

    public Task HandleAsync(AmendmentRevertedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AmendmentProjectionReducer.Apply(s, e), "AmendmentRevertedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<AmendmentReadModel, AmendmentReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new AmendmentReadModel { AmendmentId = aggregateId };
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
