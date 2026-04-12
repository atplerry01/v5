using Whyce.Shared.Contracts.Operational.Sandbox.Kanban;
using Whyce.Shared.Contracts.Engine;
using Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

namespace Whyce.Engines.T2E.Operational.Sandbox.Kanban;

public sealed class ReorderKanbanCardHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ReorderKanbanCardCommand cmd)
            return;

        var aggregate = (KanbanAggregate)await context.LoadAggregateAsync(typeof(KanbanAggregate));
        aggregate.ReorderCard(
            new KanbanCardId(cmd.CardId),
            new KanbanPosition(cmd.NewPosition));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
