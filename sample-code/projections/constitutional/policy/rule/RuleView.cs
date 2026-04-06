namespace Whycespace.Projections.Constitutional.Policy.Rule;

public sealed record RuleView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
