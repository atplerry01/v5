namespace Whyce.Shared.Contracts.Events.Operational.Sandbox.Kanban;

public sealed record KanbanCardCreatedEventSchema(Guid AggregateId, Guid CardId, Guid ListId, string Title, string Description, int Position, string? Priority);
