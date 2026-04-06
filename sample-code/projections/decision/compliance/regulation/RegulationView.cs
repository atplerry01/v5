namespace Whycespace.Projections.Decision.Compliance.Regulation;

public sealed record RegulationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
