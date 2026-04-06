namespace Whycespace.Projections.Operational.Sandbox.Todo;

public sealed record TodoReadModel
{
    public required string TodoId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Status { get; init; }
    public required int Priority { get; init; }
    public string? AssignedTo { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
}
