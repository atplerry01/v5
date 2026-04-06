namespace Whycespace.Projections.Core.Command.CommandRouting;

public sealed record CommandRoutingView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
