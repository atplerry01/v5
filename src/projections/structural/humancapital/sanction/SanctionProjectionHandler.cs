using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Humancapital.Sanction.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Sanction;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Humancapital.Sanction;

namespace Whycespace.Projections.Structural.Humancapital.Sanction;

public sealed class SanctionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<SanctionCreatedEventSchema>
{
    private readonly PostgresProjectionStore<SanctionReadModel> _store;

    public SanctionProjectionHandler(PostgresProjectionStore<SanctionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            SanctionCreatedEventSchema e => Project(e.AggregateId, s => SanctionProjectionReducer.Apply(s, e, envelope.Timestamp), "SanctionCreatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"SanctionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(SanctionCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SanctionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SanctionCreatedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<SanctionReadModel, SanctionReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new SanctionReadModel { SanctionId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
