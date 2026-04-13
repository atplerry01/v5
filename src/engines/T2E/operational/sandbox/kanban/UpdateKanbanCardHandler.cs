using Whyce.Shared.Contracts.Operational.Sandbox.Kanban;
using Whyce.Shared.Contracts.Engine;
using Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

namespace Whyce.Engines.T2E.Operational.Sandbox.Kanban;

public sealed class UpdateKanbanCardHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdateKanbanCardCommand cmd)
            return;

        var aggregate = (KanbanAggregate)await context.LoadAggregateAsync(typeof(KanbanAggregate));
        aggregate.ReviseCardDetail(
            new KanbanCardId(cmd.CardId),
            cmd.Title,
            cmd.Description);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
