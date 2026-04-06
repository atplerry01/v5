namespace Whycespace.Projections.Business.Integration.CommandBridge;

public sealed record CommandBridgeView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
