using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Humancapital.Eligibility.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Eligibility;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Humancapital.Eligibility;

namespace Whycespace.Projections.Structural.Humancapital.Eligibility;

public sealed class EligibilityProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<EligibilityCreatedEventSchema>
{
    private readonly PostgresProjectionStore<EligibilityReadModel> _store;

    public EligibilityProjectionHandler(PostgresProjectionStore<EligibilityReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            EligibilityCreatedEventSchema e => Project(e.AggregateId, s => EligibilityProjectionReducer.Apply(s, e, envelope.Timestamp), "EligibilityCreatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"EligibilityProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(EligibilityCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => EligibilityProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "EligibilityCreatedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<EligibilityReadModel, EligibilityReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new EligibilityReadModel { EligibilityId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
