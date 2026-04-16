using Whycespace.Projections.Economic.Enforcement.Escalation.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Enforcement.Escalation;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Enforcement.Escalation;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Enforcement.Escalation;

public sealed class EscalationProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<EscalationInitializedEventSchema>,
    IProjectionHandler<ViolationAccumulatedEventSchema>,
    IProjectionHandler<EscalationLevelIncreasedEventSchema>,
    IProjectionHandler<EscalationResetEventSchema>
{
    private readonly PostgresProjectionStore<EscalationReadModel> _store;

    public EscalationProjectionHandler(PostgresProjectionStore<EscalationReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            EscalationInitializedEventSchema e     => Project(e.AggregateId, s => EscalationProjectionReducer.Apply(s, e), "EscalationInitializedEvent", envelope, cancellationToken),
            ViolationAccumulatedEventSchema e      => Project(e.AggregateId, s => EscalationProjectionReducer.Apply(s, e), "ViolationAccumulatedEvent", envelope, cancellationToken),
            EscalationLevelIncreasedEventSchema e  => Project(e.AggregateId, s => EscalationProjectionReducer.Apply(s, e), "EscalationLevelIncreasedEvent", envelope, cancellationToken),
            EscalationResetEventSchema e           => Project(e.AggregateId, s => EscalationProjectionReducer.Apply(s, e), "EscalationResetEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"EscalationProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(EscalationInitializedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => EscalationProjectionReducer.Apply(s, e), "EscalationInitializedEvent", null, ct);

    public Task HandleAsync(ViolationAccumulatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => EscalationProjectionReducer.Apply(s, e), "ViolationAccumulatedEvent", null, ct);

    public Task HandleAsync(EscalationLevelIncreasedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => EscalationProjectionReducer.Apply(s, e), "EscalationLevelIncreasedEvent", null, ct);

    public Task HandleAsync(EscalationResetEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => EscalationProjectionReducer.Apply(s, e), "EscalationResetEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<EscalationReadModel, EscalationReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new EscalationReadModel { SubjectId = aggregateId };
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
