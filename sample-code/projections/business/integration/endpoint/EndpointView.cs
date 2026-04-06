namespace Whycespace.Projections.Business.Integration.Endpoint;

public sealed record EndpointView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
