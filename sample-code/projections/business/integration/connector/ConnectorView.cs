namespace Whycespace.Projections.Business.Integration.Connector;

public sealed record ConnectorView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
