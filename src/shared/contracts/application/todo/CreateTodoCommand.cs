namespace Whyce.Shared.Contracts.Application.Todo;

public sealed record CreateTodoCommand(Guid Id, string Title);
