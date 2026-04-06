namespace Whycespace.Projections.Business.Entitlement.EntitlementGrant;

public sealed record EntitlementGrantView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
