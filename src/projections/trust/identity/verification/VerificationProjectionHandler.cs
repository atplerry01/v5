using Whycespace.Projections.Shared;
using Whycespace.Projections.Trust.Identity.Verification.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Trust.Identity.Verification;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Trust.Identity.Verification;

namespace Whycespace.Projections.Trust.Identity.Verification;

public sealed class VerificationProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<VerificationInitiatedEventSchema>,
    IProjectionHandler<VerificationPassedEventSchema>,
    IProjectionHandler<VerificationFailedEventSchema>
{
    private readonly PostgresProjectionStore<VerificationReadModel> _store;

    public VerificationProjectionHandler(PostgresProjectionStore<VerificationReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            VerificationInitiatedEventSchema e => Project(e.AggregateId, s => VerificationProjectionReducer.Apply(s, e), "VerificationInitiatedEvent", envelope, cancellationToken),
            VerificationPassedEventSchema e => Project(e.AggregateId, s => VerificationProjectionReducer.Apply(s, e), "VerificationPassedEvent", envelope, cancellationToken),
            VerificationFailedEventSchema e => Project(e.AggregateId, s => VerificationProjectionReducer.Apply(s, e), "VerificationFailedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"VerificationProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(VerificationInitiatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => VerificationProjectionReducer.Apply(s, e), "VerificationInitiatedEvent", null, ct);

    public Task HandleAsync(VerificationPassedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => VerificationProjectionReducer.Apply(s, e), "VerificationPassedEvent", null, ct);

    public Task HandleAsync(VerificationFailedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => VerificationProjectionReducer.Apply(s, e), "VerificationFailedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<VerificationReadModel, VerificationReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new VerificationReadModel { VerificationId = aggregateId };
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
