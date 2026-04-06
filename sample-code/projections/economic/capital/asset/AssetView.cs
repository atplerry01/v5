namespace Whycespace.Projections.Economic.Capital.Asset;

public sealed record AssetView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
