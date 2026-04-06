namespace Whycespace.Projections.Trust.Identity.Consent;

public sealed record ConsentView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
