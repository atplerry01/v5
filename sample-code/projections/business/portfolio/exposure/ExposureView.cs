namespace Whycespace.Projections.Business.Portfolio.Exposure;

public sealed record ExposureView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
