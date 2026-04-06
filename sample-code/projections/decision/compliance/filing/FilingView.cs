namespace Whycespace.Projections.Decision.Compliance.Filing;

public sealed record FilingView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
