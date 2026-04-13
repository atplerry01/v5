using Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.Card;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

namespace Whycespace.Engines.T2E.Operational.Sandbox.Kanban;

public sealed class CompleteKanbanCardHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CompleteKanbanCardCommand cmd)
            return;

        var aggregate = (KanbanAggregate)await context.LoadAggregateAsync(typeof(KanbanAggregate));
        aggregate.CompleteCard(new KanbanCardId(cmd.CardId));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
