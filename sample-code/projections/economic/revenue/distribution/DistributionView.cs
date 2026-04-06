namespace Whycespace.Projections.Economic.Revenue.Distribution;

public sealed record DistributionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
