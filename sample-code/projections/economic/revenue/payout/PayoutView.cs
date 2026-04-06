namespace Whycespace.Projections.Economic.Revenue.Payout;

public sealed record PayoutView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
