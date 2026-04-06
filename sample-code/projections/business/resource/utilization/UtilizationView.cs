namespace Whycespace.Projections.Business.Resource.Utilization;

public sealed record UtilizationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
