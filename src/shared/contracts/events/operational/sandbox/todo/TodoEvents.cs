namespace Whyce.Shared.Contracts.Events.Operational.Sandbox.Todo;

public sealed record TodoCreatedEventSchema(Guid AggregateId, string Title);
public sealed record TodoUpdatedEventSchema(Guid AggregateId, string Title);
public sealed record TodoCompletedEventSchema(Guid AggregateId);
