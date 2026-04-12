namespace Whyce.Shared.Contracts.Events.Kanban;

public sealed record KanbanListCreatedEventSchema(Guid AggregateId, Guid ListId, string Name, int Position);
