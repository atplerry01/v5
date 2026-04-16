using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.Card;

public sealed record CreateKanbanCardCommand(Guid Id, Guid CardId, Guid ListId, string Title, string Description, int Position) : IHasAggregateId
{
    public Guid AggregateId => Id;
}

public sealed record MoveKanbanCardCommand(Guid Id, Guid CardId, Guid FromListId, Guid ToListId, int NewPosition) : IHasAggregateId
{
    public Guid AggregateId => Id;
}

public sealed record ReorderKanbanCardCommand(Guid Id, Guid CardId, Guid ListId, int NewPosition) : IHasAggregateId
{
    public Guid AggregateId => Id;
}

public sealed record CompleteKanbanCardCommand(Guid Id, Guid CardId) : IHasAggregateId
{
    public Guid AggregateId => Id;
}

public sealed record UpdateKanbanCardCommand(Guid Id, Guid CardId, string Title, string Description) : IHasAggregateId
{
    public Guid AggregateId => Id;
}

public sealed record CreateKanbanCardIntent(Guid BoardId, Guid ListId, string Title, string Description, int Position, string UserId);

public sealed record MoveKanbanCardIntent(Guid CardId, Guid BoardId, Guid FromListId, Guid ToListId, int NewPosition, string UserId);

public sealed record ReorderKanbanCardIntent(Guid CardId, Guid BoardId, Guid ListId, int NewPosition, string UserId);

public sealed record CompleteKanbanCardIntent(Guid CardId, Guid BoardId, string UserId);

public sealed record UpdateKanbanCardIntent(Guid CardId, Guid BoardId, string Title, string Description, string UserId);
