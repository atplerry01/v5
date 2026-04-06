namespace Whycespace.Platform.Api.Decision.Governance.Quorum;

public sealed record QuorumRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record QuorumResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
