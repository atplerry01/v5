namespace Whycespace.Platform.Api.Core.Reconciliation.ReconciliationRun;

public sealed record ReconciliationRunRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ReconciliationRunResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
