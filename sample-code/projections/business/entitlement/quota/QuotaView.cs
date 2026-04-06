namespace Whycespace.Projections.Business.Entitlement.Quota;

public sealed record QuotaView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
