namespace Whycespace.Projections.Operational.Sandbox.Todo;

public sealed record TodoView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
