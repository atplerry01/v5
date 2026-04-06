namespace Whycespace.Projections.Business.Portfolio.Mandate;

public sealed record MandateView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
