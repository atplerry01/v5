namespace Whycespace.Projections.Core.State.StateTransition;

public sealed record StateTransitionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
