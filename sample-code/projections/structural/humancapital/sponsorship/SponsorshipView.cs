namespace Whycespace.Projections.Structural.Humancapital.Sponsorship;

public sealed record SponsorshipView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
