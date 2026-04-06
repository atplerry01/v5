namespace Whycespace.Projections.Business.Billing.Adjustment;

public sealed record AdjustmentView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
