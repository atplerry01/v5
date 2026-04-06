namespace Whycespace.Projections.Business.Integration.Gateway;

public sealed record GatewayView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
