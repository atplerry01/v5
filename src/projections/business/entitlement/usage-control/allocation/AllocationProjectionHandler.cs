using Whycespace.Projections.Business.Entitlement.UsageControl.Allocation.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.Allocation;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Entitlement.UsageControl.Allocation;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Entitlement.UsageControl.Allocation;

public sealed class AllocationProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<AllocationCreatedEventSchema>,
    IProjectionHandler<AllocationAllocatedEventSchema>,
    IProjectionHandler<AllocationReleasedEventSchema>
{
    private readonly PostgresProjectionStore<AllocationReadModel> _store;

    public AllocationProjectionHandler(PostgresProjectionStore<AllocationReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            AllocationCreatedEventSchema e   => Project(e.AggregateId, s => AllocationProjectionReducer.Apply(s, e), "AllocationCreatedEvent",   envelope, cancellationToken),
            AllocationAllocatedEventSchema e => Project(e.AggregateId, s => AllocationProjectionReducer.Apply(s, e), "AllocationAllocatedEvent", envelope, cancellationToken),
            AllocationReleasedEventSchema e  => Project(e.AggregateId, s => AllocationProjectionReducer.Apply(s, e), "AllocationReleasedEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"AllocationProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(AllocationCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AllocationProjectionReducer.Apply(s, e), "AllocationCreatedEvent", null, ct);

    public Task HandleAsync(AllocationAllocatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AllocationProjectionReducer.Apply(s, e), "AllocationAllocatedEvent", null, ct);

    public Task HandleAsync(AllocationReleasedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => AllocationProjectionReducer.Apply(s, e), "AllocationReleasedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<AllocationReadModel, AllocationReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new AllocationReadModel { AllocationId = aggregateId };
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
