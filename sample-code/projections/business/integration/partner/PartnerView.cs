namespace Whycespace.Projections.Business.Integration.Partner;

public sealed record PartnerView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
