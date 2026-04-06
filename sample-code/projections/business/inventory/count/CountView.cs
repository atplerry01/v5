namespace Whycespace.Projections.Business.Inventory.Count;

public sealed record CountView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
