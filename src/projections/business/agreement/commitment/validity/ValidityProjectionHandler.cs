using Whycespace.Projections.Business.Agreement.Commitment.Validity.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Validity;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Agreement.Commitment.Validity;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Agreement.Commitment.Validity;

public sealed class ValidityProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ValidityCreatedEventSchema>,
    IProjectionHandler<ValidityExpiredEventSchema>,
    IProjectionHandler<ValidityInvalidatedEventSchema>
{
    private readonly PostgresProjectionStore<ValidityReadModel> _store;

    public ValidityProjectionHandler(PostgresProjectionStore<ValidityReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ValidityCreatedEventSchema e     => Project(e.AggregateId, s => ValidityProjectionReducer.Apply(s, e), "ValidityCreatedEvent",     envelope, cancellationToken),
            ValidityExpiredEventSchema e     => Project(e.AggregateId, s => ValidityProjectionReducer.Apply(s, e), "ValidityExpiredEvent",     envelope, cancellationToken),
            ValidityInvalidatedEventSchema e => Project(e.AggregateId, s => ValidityProjectionReducer.Apply(s, e), "ValidityInvalidatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ValidityProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ValidityCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ValidityProjectionReducer.Apply(s, e), "ValidityCreatedEvent", null, ct);

    public Task HandleAsync(ValidityExpiredEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ValidityProjectionReducer.Apply(s, e), "ValidityExpiredEvent", null, ct);

    public Task HandleAsync(ValidityInvalidatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ValidityProjectionReducer.Apply(s, e), "ValidityInvalidatedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ValidityReadModel, ValidityReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ValidityReadModel { ValidityId = aggregateId };
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
