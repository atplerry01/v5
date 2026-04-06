namespace Whycespace.Projections.Core.Reconciliation.ReconciliationReport;

public sealed record ReconciliationReportView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
