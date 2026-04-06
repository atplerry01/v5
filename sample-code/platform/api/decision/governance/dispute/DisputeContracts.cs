namespace Whycespace.Platform.Api.Decision.Governance.Dispute;

public sealed record DisputeRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record DisputeResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
