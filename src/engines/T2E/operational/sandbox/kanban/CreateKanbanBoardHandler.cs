using Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.Board;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

namespace Whycespace.Engines.T2E.Operational.Sandbox.Kanban;

public sealed class CreateKanbanBoardHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateKanbanBoardCommand cmd)
            return Task.CompletedTask;

        var aggregate = KanbanAggregate.Create(
            new KanbanBoardId(context.AggregateId),
            cmd.Name);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
