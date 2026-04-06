namespace Whycespace.Platform.Api.Operational.Sandbox.Todo.Contracts;

public sealed record CreateTodoRequest
{
    public string? TodoId { get; init; }
    public required string Title { get; init; }
    public int Priority { get; init; }
    public string? AssignedTo { get; init; }
}
