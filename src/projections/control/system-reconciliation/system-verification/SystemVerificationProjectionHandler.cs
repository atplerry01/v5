using Whycespace.Projections.Control.SystemReconciliation.SystemVerification.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.SystemVerification;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.SystemReconciliation.SystemVerification;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.SystemReconciliation.SystemVerification;

public sealed class SystemVerificationProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<SystemVerificationInitiatedEventSchema>,
    IProjectionHandler<SystemVerificationPassedEventSchema>,
    IProjectionHandler<SystemVerificationFailedEventSchema>
{
    private readonly PostgresProjectionStore<SystemVerificationReadModel> _store;

    public SystemVerificationProjectionHandler(PostgresProjectionStore<SystemVerificationReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            SystemVerificationInitiatedEventSchema e  => Project(e.AggregateId, s => SystemVerificationProjectionReducer.Apply(s, e), "SystemVerificationInitiatedEvent",  envelope, cancellationToken),
            SystemVerificationPassedEventSchema e     => Project(e.AggregateId, s => SystemVerificationProjectionReducer.Apply(s, e), "SystemVerificationPassedEvent",     envelope, cancellationToken),
            SystemVerificationFailedEventSchema e     => Project(e.AggregateId, s => SystemVerificationProjectionReducer.Apply(s, e), "SystemVerificationFailedEvent",     envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"SystemVerificationProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(SystemVerificationInitiatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SystemVerificationProjectionReducer.Apply(s, e), "SystemVerificationInitiatedEvent", null, ct);

    public Task HandleAsync(SystemVerificationPassedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SystemVerificationProjectionReducer.Apply(s, e), "SystemVerificationPassedEvent", null, ct);

    public Task HandleAsync(SystemVerificationFailedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SystemVerificationProjectionReducer.Apply(s, e), "SystemVerificationFailedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<SystemVerificationReadModel, SystemVerificationReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new SystemVerificationReadModel { VerificationId = aggregateId };
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
