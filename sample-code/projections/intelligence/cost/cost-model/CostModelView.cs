namespace Whycespace.Projections.Intelligence.Cost.CostModel;

public sealed record CostModelView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
