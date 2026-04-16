using Whycespace.Projections.Economic.Enforcement.Violation.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Enforcement.Violation;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Enforcement.Violation;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Enforcement.Violation;

public sealed class ViolationProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ViolationDetectedEventSchema>,
    IProjectionHandler<ViolationAcknowledgedEventSchema>,
    IProjectionHandler<ViolationResolvedEventSchema>
{
    private readonly PostgresProjectionStore<ViolationReadModel> _store;

    public ViolationProjectionHandler(PostgresProjectionStore<ViolationReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ViolationDetectedEventSchema e     => Project(e.AggregateId, s => ViolationProjectionReducer.Apply(s, e), "ViolationDetectedEvent", envelope, cancellationToken),
            ViolationAcknowledgedEventSchema e => Project(e.AggregateId, s => ViolationProjectionReducer.Apply(s, e), "ViolationAcknowledgedEvent", envelope, cancellationToken),
            ViolationResolvedEventSchema e     => Project(e.AggregateId, s => ViolationProjectionReducer.Apply(s, e), "ViolationResolvedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ViolationProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ViolationDetectedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ViolationProjectionReducer.Apply(s, e), "ViolationDetectedEvent", null, ct);

    public Task HandleAsync(ViolationAcknowledgedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ViolationProjectionReducer.Apply(s, e), "ViolationAcknowledgedEvent", null, ct);

    public Task HandleAsync(ViolationResolvedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ViolationProjectionReducer.Apply(s, e), "ViolationResolvedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ViolationReadModel, ViolationReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ViolationReadModel { ViolationId = aggregateId };
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
