namespace Whycespace.Projections.Business.Entitlement.Revocation;

public sealed record RevocationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
