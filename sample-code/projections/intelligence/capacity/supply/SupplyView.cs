namespace Whycespace.Projections.Intelligence.Capacity.Supply;

public sealed record SupplyView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
