namespace Whycespace.Projections.Business.Integration.Client;

public sealed record ClientView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
