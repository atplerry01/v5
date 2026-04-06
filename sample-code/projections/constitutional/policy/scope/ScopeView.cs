namespace Whycespace.Projections.Constitutional.Policy.Scope;

public sealed record ScopeView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
