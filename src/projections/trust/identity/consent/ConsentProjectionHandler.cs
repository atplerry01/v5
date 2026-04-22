using Whycespace.Projections.Shared;
using Whycespace.Projections.Trust.Identity.Consent.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Trust.Identity.Consent;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Trust.Identity.Consent;

namespace Whycespace.Projections.Trust.Identity.Consent;

public sealed class ConsentProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ConsentGrantedEventSchema>,
    IProjectionHandler<ConsentRevokedEventSchema>,
    IProjectionHandler<ConsentExpiredEventSchema>
{
    private readonly PostgresProjectionStore<ConsentReadModel> _store;

    public ConsentProjectionHandler(PostgresProjectionStore<ConsentReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ConsentGrantedEventSchema e => Project(e.AggregateId, s => ConsentProjectionReducer.Apply(s, e), "ConsentGrantedEvent", envelope, cancellationToken),
            ConsentRevokedEventSchema e => Project(e.AggregateId, s => ConsentProjectionReducer.Apply(s, e), "ConsentRevokedEvent", envelope, cancellationToken),
            ConsentExpiredEventSchema e => Project(e.AggregateId, s => ConsentProjectionReducer.Apply(s, e), "ConsentExpiredEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ConsentProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ConsentGrantedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ConsentProjectionReducer.Apply(s, e), "ConsentGrantedEvent", null, ct);

    public Task HandleAsync(ConsentRevokedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ConsentProjectionReducer.Apply(s, e), "ConsentRevokedEvent", null, ct);

    public Task HandleAsync(ConsentExpiredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ConsentProjectionReducer.Apply(s, e), "ConsentExpiredEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ConsentReadModel, ConsentReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ConsentReadModel { ConsentId = aggregateId };
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
