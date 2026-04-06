namespace Whycespace.Platform.Api.Structural.Humancapital.Governance;

public sealed record GovernanceRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record GovernanceResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
