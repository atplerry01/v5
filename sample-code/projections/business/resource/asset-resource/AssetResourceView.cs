namespace Whycespace.Projections.Business.Resource.AssetResource;

public sealed record AssetResourceView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
