using Whycespace.Projections.Operational.Sandbox.Kanban.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Operational.Sandbox.Kanban.Board;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.Board;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Operational.Sandbox.Kanban.Board;

public sealed class KanbanBoardProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<KanbanBoardCreatedEventSchema>
{
    private readonly PostgresProjectionStore<KanbanBoardReadModel> _store;

    public KanbanBoardProjectionHandler(PostgresProjectionStore<KanbanBoardReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            KanbanBoardCreatedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            _ => throw new InvalidOperationException(
                $"KanbanBoardProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public async Task HandleAsync(KanbanBoardCreatedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    private async Task Project(KanbanBoardCreatedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new KanbanBoardReadModel { BoardId = e.AggregateId };
        state = KanbanProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "KanbanBoardCreatedEvent", eventId, eventVersion, correlationId, ct);
    }
}
