namespace Whycespace.Projections.Intelligence.Index.RiskIndex;

public sealed record RiskIndexView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
