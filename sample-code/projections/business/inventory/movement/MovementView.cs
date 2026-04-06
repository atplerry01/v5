namespace Whycespace.Projections.Business.Inventory.Movement;

public sealed record MovementView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
