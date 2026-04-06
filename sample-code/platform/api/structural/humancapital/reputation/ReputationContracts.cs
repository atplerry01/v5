namespace Whycespace.Platform.Api.Structural.Humancapital.Reputation;

public sealed record ReputationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ReputationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
