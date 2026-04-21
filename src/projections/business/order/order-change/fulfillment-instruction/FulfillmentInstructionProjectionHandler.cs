using Whycespace.Projections.Business.Order.OrderChange.FulfillmentInstruction.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Order.OrderChange.FulfillmentInstruction;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Order.OrderChange.FulfillmentInstruction;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Order.OrderChange.FulfillmentInstruction;

public sealed class FulfillmentInstructionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<FulfillmentInstructionCreatedEventSchema>,
    IProjectionHandler<FulfillmentInstructionIssuedEventSchema>,
    IProjectionHandler<FulfillmentInstructionCompletedEventSchema>,
    IProjectionHandler<FulfillmentInstructionRevokedEventSchema>
{
    private readonly PostgresProjectionStore<FulfillmentInstructionReadModel> _store;

    public FulfillmentInstructionProjectionHandler(PostgresProjectionStore<FulfillmentInstructionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            FulfillmentInstructionCreatedEventSchema   e => Project(e.AggregateId, s => FulfillmentInstructionProjectionReducer.Apply(s, e), "FulfillmentInstructionCreatedEvent",   envelope, cancellationToken),
            FulfillmentInstructionIssuedEventSchema    e => Project(e.AggregateId, s => FulfillmentInstructionProjectionReducer.Apply(s, e), "FulfillmentInstructionIssuedEvent",    envelope, cancellationToken),
            FulfillmentInstructionCompletedEventSchema e => Project(e.AggregateId, s => FulfillmentInstructionProjectionReducer.Apply(s, e), "FulfillmentInstructionCompletedEvent", envelope, cancellationToken),
            FulfillmentInstructionRevokedEventSchema   e => Project(e.AggregateId, s => FulfillmentInstructionProjectionReducer.Apply(s, e), "FulfillmentInstructionRevokedEvent",   envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"FulfillmentInstructionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(FulfillmentInstructionCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => FulfillmentInstructionProjectionReducer.Apply(s, e), "FulfillmentInstructionCreatedEvent", null, ct);

    public Task HandleAsync(FulfillmentInstructionIssuedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => FulfillmentInstructionProjectionReducer.Apply(s, e), "FulfillmentInstructionIssuedEvent", null, ct);

    public Task HandleAsync(FulfillmentInstructionCompletedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => FulfillmentInstructionProjectionReducer.Apply(s, e), "FulfillmentInstructionCompletedEvent", null, ct);

    public Task HandleAsync(FulfillmentInstructionRevokedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => FulfillmentInstructionProjectionReducer.Apply(s, e), "FulfillmentInstructionRevokedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<FulfillmentInstructionReadModel, FulfillmentInstructionReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new FulfillmentInstructionReadModel { FulfillmentInstructionId = aggregateId };
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
