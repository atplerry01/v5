namespace Whyce.Shared.Contracts.Application.Todo;

public sealed record UpdateTodoCommand(Guid Id, string Title);
