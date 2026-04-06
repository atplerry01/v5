namespace Whycespace.Projections.Business.Inventory.Transfer;

public sealed record TransferView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
