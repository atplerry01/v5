namespace Whyce.Shared.Contracts.Events.Operational.Sandbox.Kanban.Card;

public sealed record KanbanCardCreatedEventSchema(Guid AggregateId, Guid CardId, Guid ListId, string Title, string Description, int Position, string? Priority);

public sealed record KanbanCardMovedEventSchema(Guid AggregateId, Guid CardId, Guid FromListId, Guid ToListId, int NewPosition);

public sealed record KanbanCardReorderedEventSchema(Guid AggregateId, Guid CardId, Guid ListId, int NewPosition);

public sealed record KanbanCardCompletedEventSchema(Guid AggregateId, Guid CardId);

public sealed record KanbanCardUpdatedEventSchema(Guid AggregateId, Guid CardId, string Title, string Description);
