namespace Whycespace.Projections.Decision.Governance.Charter;

public sealed record CharterView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
