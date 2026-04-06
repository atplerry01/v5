namespace Whycespace.Projections.Decision.Compliance.Jurisdiction;

public sealed record JurisdictionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
