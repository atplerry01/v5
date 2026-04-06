namespace Whycespace.Projections.Intelligence.Relationship.Affiliation;

public sealed record AffiliationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
