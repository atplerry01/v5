namespace Whycespace.Projections.Business.Integration.Transformation;

public sealed record TransformationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
