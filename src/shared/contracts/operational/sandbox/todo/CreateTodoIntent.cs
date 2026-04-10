namespace Whyce.Shared.Contracts.Operational.Sandbox.Todo;

public sealed record CreateTodoIntent(
    string Title,
    string Description,
    string UserId);
