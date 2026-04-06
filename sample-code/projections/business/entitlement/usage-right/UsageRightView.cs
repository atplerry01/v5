namespace Whycespace.Projections.Business.Entitlement.UsageRight;

public sealed record UsageRightView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
