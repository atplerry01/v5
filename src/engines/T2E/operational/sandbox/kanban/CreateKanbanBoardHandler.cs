using Whyce.Shared.Contracts.Operational.Sandbox.Kanban;
using Whyce.Shared.Contracts.Engine;
using Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

namespace Whyce.Engines.T2E.Operational.Sandbox.Kanban;

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
