namespace Whycespace.Platform.Api.Decision.Governance.GovernanceRecord;

public sealed record GovernanceRecordRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record GovernanceRecordResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
