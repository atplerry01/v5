namespace Whycespace.Projections.Structural.Humancapital.Sanction;

public sealed record SanctionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
