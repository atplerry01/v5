namespace Whycespace.Projections.Business.Integration.Job;

public sealed record JobView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
