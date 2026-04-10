namespace Whyce.Shared.Contracts.Operational.Sandbox.Todo;

public sealed record UpdateTodoCommand(Guid Id, string Title);
