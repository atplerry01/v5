namespace Whycespace.Platform.Api.Decision.Governance.Guardian;

public sealed record GuardianRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record GuardianResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
