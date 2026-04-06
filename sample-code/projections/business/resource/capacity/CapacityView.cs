namespace Whycespace.Projections.Business.Resource.Capacity;

public sealed record CapacityView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
