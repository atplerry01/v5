using Whycespace.Projections.Economic.Capital.Binding.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Capital.Binding;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Capital.Binding;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Capital.Binding;

public sealed class CapitalBindingProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<CapitalBoundEventSchema>,
    IProjectionHandler<OwnershipTransferredEventSchema>,
    IProjectionHandler<BindingReleasedEventSchema>
{
    private readonly PostgresProjectionStore<CapitalBindingReadModel> _store;

    public CapitalBindingProjectionHandler(PostgresProjectionStore<CapitalBindingReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            CapitalBoundEventSchema e => Project(e.AggregateId, s => CapitalBindingProjectionReducer.Apply(s, e), "CapitalBoundEvent", envelope, cancellationToken),
            OwnershipTransferredEventSchema e => Project(e.AggregateId, s => CapitalBindingProjectionReducer.Apply(s, e), "OwnershipTransferredEvent", envelope, cancellationToken),
            BindingReleasedEventSchema e => Project(e.AggregateId, s => CapitalBindingProjectionReducer.Apply(s, e), "BindingReleasedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"CapitalBindingProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(CapitalBoundEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalBindingProjectionReducer.Apply(s, e), "CapitalBoundEvent", null, ct);

    public Task HandleAsync(OwnershipTransferredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalBindingProjectionReducer.Apply(s, e), "OwnershipTransferredEvent", null, ct);

    public Task HandleAsync(BindingReleasedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalBindingProjectionReducer.Apply(s, e), "BindingReleasedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<CapitalBindingReadModel, CapitalBindingReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new CapitalBindingReadModel { BindingId = aggregateId };
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
