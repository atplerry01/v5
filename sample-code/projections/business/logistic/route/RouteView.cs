namespace Whycespace.Projections.Business.Logistic.Route;

public sealed record RouteView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
