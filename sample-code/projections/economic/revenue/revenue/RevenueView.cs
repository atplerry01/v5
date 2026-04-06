namespace Whycespace.Projections.Economic.Revenue.Revenue;

public sealed record RevenueView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
