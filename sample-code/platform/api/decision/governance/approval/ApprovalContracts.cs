namespace Whycespace.Platform.Api.Decision.Governance.Approval;

public sealed record ApprovalRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ApprovalResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
