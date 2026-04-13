namespace Whyce.Shared.Contracts.Events.Operational.Sandbox.Kanban;

public sealed record KanbanListCreatedEventSchema(Guid AggregateId, Guid ListId, string Name, int Position);
