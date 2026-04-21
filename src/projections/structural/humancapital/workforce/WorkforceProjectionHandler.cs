using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Humancapital.Workforce.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Workforce;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Humancapital.Workforce;

namespace Whycespace.Projections.Structural.Humancapital.Workforce;

public sealed class WorkforceProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<WorkforceCreatedEventSchema>
{
    private readonly PostgresProjectionStore<WorkforceReadModel> _store;

    public WorkforceProjectionHandler(PostgresProjectionStore<WorkforceReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            WorkforceCreatedEventSchema e => Project(e.AggregateId, s => WorkforceProjectionReducer.Apply(s, e, envelope.Timestamp), "WorkforceCreatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"WorkforceProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(WorkforceCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => WorkforceProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "WorkforceCreatedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<WorkforceReadModel, WorkforceReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new WorkforceReadModel { WorkforceId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
