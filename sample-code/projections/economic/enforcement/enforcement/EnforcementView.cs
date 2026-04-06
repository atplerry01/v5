namespace Whycespace.Projections.Economic.Enforcement.Enforcement;

public sealed record EnforcementView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
