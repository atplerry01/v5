namespace Whycespace.Platform.Api.Decision.Governance.Delegation;

public sealed record DelegationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record DelegationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
