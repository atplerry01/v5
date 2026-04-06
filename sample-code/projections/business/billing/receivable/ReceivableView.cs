namespace Whycespace.Projections.Business.Billing.Receivable;

public sealed record ReceivableView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
