namespace Whycespace.Projections.Business.Inventory.Item;

public sealed record ItemView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
