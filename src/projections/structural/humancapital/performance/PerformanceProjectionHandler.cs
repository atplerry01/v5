using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Humancapital.Performance.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Performance;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Humancapital.Performance;

namespace Whycespace.Projections.Structural.Humancapital.Performance;

public sealed class PerformanceProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<PerformanceCreatedEventSchema>
{
    private readonly PostgresProjectionStore<PerformanceReadModel> _store;

    public PerformanceProjectionHandler(PostgresProjectionStore<PerformanceReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            PerformanceCreatedEventSchema e => Project(e.AggregateId, s => PerformanceProjectionReducer.Apply(s, e, envelope.Timestamp), "PerformanceCreatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"PerformanceProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(PerformanceCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => PerformanceProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "PerformanceCreatedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<PerformanceReadModel, PerformanceReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new PerformanceReadModel { PerformanceId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
