namespace Whycespace.Projections.Intelligence.Index.PriceIndex;

public sealed record PriceIndexView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
