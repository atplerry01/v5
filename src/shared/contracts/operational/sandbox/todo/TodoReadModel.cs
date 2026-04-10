namespace Whyce.Shared.Contracts.Operational.Sandbox.Todo;

public sealed record TodoReadModel
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public bool IsCompleted { get; init; }
    public string Status { get; init; } = "active";
}
