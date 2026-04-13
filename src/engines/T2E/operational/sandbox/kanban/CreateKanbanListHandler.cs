using Whyce.Shared.Contracts.Operational.Sandbox.Kanban.List;
using Whyce.Shared.Contracts.Engine;
using Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

namespace Whyce.Engines.T2E.Operational.Sandbox.Kanban;

public sealed class CreateKanbanListHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateKanbanListCommand cmd)
            return;

        var aggregate = (KanbanAggregate)await context.LoadAggregateAsync(typeof(KanbanAggregate));
        aggregate.CreateList(
            new KanbanListId(cmd.ListId),
            cmd.Name,
            new KanbanPosition(cmd.Position));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
