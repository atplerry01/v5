using Whycespace.Projections.Business.Entitlement.EligibilityAndGrant.Grant.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Grant;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Entitlement.EligibilityAndGrant.Grant;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Entitlement.EligibilityAndGrant.Grant;

public sealed class GrantProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<GrantCreatedEventSchema>,
    IProjectionHandler<GrantActivatedEventSchema>,
    IProjectionHandler<GrantRevokedEventSchema>,
    IProjectionHandler<GrantExpiredEventSchema>
{
    private readonly PostgresProjectionStore<GrantReadModel> _store;

    public GrantProjectionHandler(PostgresProjectionStore<GrantReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            GrantCreatedEventSchema e   => Project(e.AggregateId, s => GrantProjectionReducer.Apply(s, e), "GrantCreatedEvent",   envelope, cancellationToken),
            GrantActivatedEventSchema e => Project(e.AggregateId, s => GrantProjectionReducer.Apply(s, e), "GrantActivatedEvent", envelope, cancellationToken),
            GrantRevokedEventSchema e   => Project(e.AggregateId, s => GrantProjectionReducer.Apply(s, e), "GrantRevokedEvent",   envelope, cancellationToken),
            GrantExpiredEventSchema e   => Project(e.AggregateId, s => GrantProjectionReducer.Apply(s, e), "GrantExpiredEvent",   envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"GrantProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(GrantCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => GrantProjectionReducer.Apply(s, e), "GrantCreatedEvent", null, ct);

    public Task HandleAsync(GrantActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => GrantProjectionReducer.Apply(s, e), "GrantActivatedEvent", null, ct);

    public Task HandleAsync(GrantRevokedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => GrantProjectionReducer.Apply(s, e), "GrantRevokedEvent", null, ct);

    public Task HandleAsync(GrantExpiredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => GrantProjectionReducer.Apply(s, e), "GrantExpiredEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<GrantReadModel, GrantReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new GrantReadModel { GrantId = aggregateId };
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
