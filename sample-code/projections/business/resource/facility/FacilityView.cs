namespace Whycespace.Projections.Business.Resource.Facility;

public sealed record FacilityView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
