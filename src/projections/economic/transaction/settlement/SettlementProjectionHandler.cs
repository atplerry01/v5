using Whycespace.Projections.Economic.Transaction.Settlement.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Transaction.Settlement;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Transaction.Settlement;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Transaction.Settlement;

public sealed class SettlementProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<SettlementInitiatedEventSchema>,
    IProjectionHandler<SettlementProcessingStartedEventSchema>,
    IProjectionHandler<SettlementCompletedEventSchema>,
    IProjectionHandler<SettlementFailedEventSchema>
{
    private readonly PostgresProjectionStore<SettlementReadModel> _store;

    public SettlementProjectionHandler(PostgresProjectionStore<SettlementReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            SettlementInitiatedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            SettlementProcessingStartedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            SettlementCompletedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            SettlementFailedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            _ => throw new InvalidOperationException(
                $"SettlementProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public async Task HandleAsync(SettlementInitiatedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    public async Task HandleAsync(SettlementProcessingStartedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    public async Task HandleAsync(SettlementCompletedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    public async Task HandleAsync(SettlementFailedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    private async Task Project(SettlementInitiatedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new SettlementReadModel { SettlementId = e.AggregateId };
        state = SettlementProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "SettlementInitiatedEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task Project(SettlementProcessingStartedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new SettlementReadModel { SettlementId = e.AggregateId };
        state = SettlementProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "SettlementProcessingStartedEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task Project(SettlementCompletedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new SettlementReadModel { SettlementId = e.AggregateId };
        state = SettlementProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "SettlementCompletedEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task Project(SettlementFailedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new SettlementReadModel { SettlementId = e.AggregateId };
        state = SettlementProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "SettlementFailedEvent", eventId, eventVersion, correlationId, ct);
    }
}
