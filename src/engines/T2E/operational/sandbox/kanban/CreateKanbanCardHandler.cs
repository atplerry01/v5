using Whyce.Shared.Contracts.Operational.Sandbox.Kanban.Card;
using Whyce.Shared.Contracts.Engine;
using Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

namespace Whyce.Engines.T2E.Operational.Sandbox.Kanban;

public sealed class CreateKanbanCardHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateKanbanCardCommand cmd)
            return;

        var aggregate = (KanbanAggregate)await context.LoadAggregateAsync(typeof(KanbanAggregate));
        aggregate.CreateCard(
            new KanbanCardId(cmd.CardId),
            new KanbanListId(cmd.ListId),
            cmd.Title,
            cmd.Description,
            new KanbanPosition(cmd.Position));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
