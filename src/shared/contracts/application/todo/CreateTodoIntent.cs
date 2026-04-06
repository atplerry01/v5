namespace Whyce.Shared.Contracts.Application.Todo;

public sealed record CreateTodoIntent(
    string Title,
    string Description,
    string UserId);
