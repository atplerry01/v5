using Whycespace.Projections.Economic.Exchange.Fx.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Exchange.Fx;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Exchange.Fx;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Exchange.Fx;

public sealed class FxProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<FxPairRegisteredEventSchema>,
    IProjectionHandler<FxPairActivatedEventSchema>,
    IProjectionHandler<FxPairDeactivatedEventSchema>
{
    private readonly PostgresProjectionStore<FxReadModel> _store;

    public FxProjectionHandler(PostgresProjectionStore<FxReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            FxPairRegisteredEventSchema e  => Project(e.AggregateId, s => FxProjectionReducer.Apply(s, e, envelope.Timestamp), "FxPairRegisteredEvent",  envelope, cancellationToken),
            FxPairActivatedEventSchema e   => Project(e.AggregateId, s => FxProjectionReducer.Apply(s, e), "FxPairActivatedEvent",   envelope, cancellationToken),
            FxPairDeactivatedEventSchema e => Project(e.AggregateId, s => FxProjectionReducer.Apply(s, e), "FxPairDeactivatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"FxProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    // Direct (envelope-less) entry points. Registration timestamp cannot be
    // recovered without the envelope on these paths, so we stamp Clock-less
    // MinValue explicitly and defer to the canonical envelope path for the
    // authoritative value. In practice these overloads are only used by
    // the legacy IProjectionHandler<TSchema> dispatchers.
    public Task HandleAsync(FxPairRegisteredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => FxProjectionReducer.Apply(s, e, DateTimeOffset.MinValue), "FxPairRegisteredEvent", null, ct);

    public Task HandleAsync(FxPairActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => FxProjectionReducer.Apply(s, e), "FxPairActivatedEvent", null, ct);

    public Task HandleAsync(FxPairDeactivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => FxProjectionReducer.Apply(s, e), "FxPairDeactivatedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<FxReadModel, FxReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new FxReadModel { FxId = aggregateId };
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
