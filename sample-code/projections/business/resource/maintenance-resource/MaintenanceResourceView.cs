namespace Whycespace.Projections.Business.Resource.MaintenanceResource;

public sealed record MaintenanceResourceView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
