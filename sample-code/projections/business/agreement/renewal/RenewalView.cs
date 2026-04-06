namespace Whycespace.Projections.Business.Agreement.Renewal;

public sealed record RenewalView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
