namespace Whycespace.Projections.Business.Entitlement.Limit;

public sealed record LimitView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
