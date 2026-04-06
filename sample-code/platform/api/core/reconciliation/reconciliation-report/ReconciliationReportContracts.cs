namespace Whycespace.Platform.Api.Core.Reconciliation.ReconciliationReport;

public sealed record ReconciliationReportRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ReconciliationReportResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
