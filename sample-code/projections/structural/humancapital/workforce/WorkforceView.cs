namespace Whycespace.Projections.Structural.Humancapital.Workforce;

public sealed record WorkforceView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
