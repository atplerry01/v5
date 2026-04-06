namespace Whycespace.Projections.Business.Integration.Failure;

public sealed record FailureView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
