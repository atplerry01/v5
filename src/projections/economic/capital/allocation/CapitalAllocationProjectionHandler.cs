using Whycespace.Projections.Economic.Capital.Allocation.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Capital.Allocation;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Capital.Allocation;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Capital.Allocation;

public sealed class CapitalAllocationProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<AllocationCreatedEventSchema>,
    IProjectionHandler<AllocationReleasedEventSchema>,
    IProjectionHandler<AllocationCompletedEventSchema>,
    IProjectionHandler<CapitalAllocatedToSpvEventSchema>
{
    private readonly PostgresProjectionStore<CapitalAllocationReadModel> _store;

    public CapitalAllocationProjectionHandler(PostgresProjectionStore<CapitalAllocationReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            AllocationCreatedEventSchema e => Project(e.AggregateId, s => CapitalAllocationProjectionReducer.Apply(s, e), "AllocationCreatedEvent", envelope, cancellationToken),
            AllocationReleasedEventSchema e => Project(e.AggregateId, s => CapitalAllocationProjectionReducer.Apply(s, e), "AllocationReleasedEvent", envelope, cancellationToken),
            AllocationCompletedEventSchema e => Project(e.AggregateId, s => CapitalAllocationProjectionReducer.Apply(s, e), "AllocationCompletedEvent", envelope, cancellationToken),
            CapitalAllocatedToSpvEventSchema e => Project(e.AggregateId, s => CapitalAllocationProjectionReducer.Apply(s, e), "CapitalAllocatedToSpvEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"CapitalAllocationProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(AllocationCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalAllocationProjectionReducer.Apply(s, e), "AllocationCreatedEvent", null, ct);

    public Task HandleAsync(AllocationReleasedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalAllocationProjectionReducer.Apply(s, e), "AllocationReleasedEvent", null, ct);

    public Task HandleAsync(AllocationCompletedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalAllocationProjectionReducer.Apply(s, e), "AllocationCompletedEvent", null, ct);

    public Task HandleAsync(CapitalAllocatedToSpvEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalAllocationProjectionReducer.Apply(s, e), "CapitalAllocatedToSpvEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<CapitalAllocationReadModel, CapitalAllocationReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new CapitalAllocationReadModel { AllocationId = aggregateId };
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
