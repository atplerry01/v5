namespace Whycespace.Projections.Business.Billing.BillRun;

public sealed record BillRunView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
