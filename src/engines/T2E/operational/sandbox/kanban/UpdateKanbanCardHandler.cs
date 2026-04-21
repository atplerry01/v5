using Whycespace.Domain.OperationalSystem.Sandbox.Kanban;
using Whycespace.Domain.SharedKernel.Primitive.Identity;
using Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.Card;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Operational.Sandbox.Kanban;

public sealed class UpdateKanbanCardHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdateKanbanCardCommand cmd)
            return;

        if (!ContentId.TryParse(cmd.Description, out var descriptionContentId))
            throw new InvalidOperationException(KanbanDomainErrors.InvalidCardDescriptionRef);

        var aggregate = (KanbanAggregate)await context.LoadAggregateAsync(typeof(KanbanAggregate));
        aggregate.ReviseCardDetail(
            new KanbanCardId(cmd.CardId),
            new KanbanCardTitle(cmd.Title),
            new DocumentRef(descriptionContentId));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
