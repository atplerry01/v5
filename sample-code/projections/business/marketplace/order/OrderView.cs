namespace Whycespace.Projections.Business.Marketplace.Order;

public sealed record OrderView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
