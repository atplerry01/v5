namespace Whycespace.Projections.Business.Inventory.Writeoff;

public sealed record WriteoffView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
