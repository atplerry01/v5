using Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.Card;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

namespace Whycespace.Engines.T2E.Operational.Sandbox.Kanban;

public sealed class MoveKanbanCardHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not MoveKanbanCardCommand cmd)
            return;

        var aggregate = (KanbanAggregate)await context.LoadAggregateAsync(typeof(KanbanAggregate));
        aggregate.MoveCard(
            new KanbanCardId(cmd.CardId),
            new KanbanListId(cmd.ToListId),
            new KanbanPosition(cmd.NewPosition));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
