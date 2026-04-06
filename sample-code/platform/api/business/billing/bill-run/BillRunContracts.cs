namespace Whycespace.Platform.Api.Business.Billing.BillRun;

public sealed record BillRunRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record BillRunResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
