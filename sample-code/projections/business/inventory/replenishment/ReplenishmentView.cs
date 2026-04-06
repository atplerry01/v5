namespace Whycespace.Projections.Business.Inventory.Replenishment;

public sealed record ReplenishmentView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
