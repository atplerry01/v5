namespace Whycespace.Projections.Business.Portfolio.Portfolio;

public sealed record PortfolioView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
