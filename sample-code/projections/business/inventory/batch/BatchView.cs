namespace Whycespace.Projections.Business.Inventory.Batch;

public sealed record BatchView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
