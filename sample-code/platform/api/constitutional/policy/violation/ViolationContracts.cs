namespace Whycespace.Platform.Api.Constitutional.Policy.Violation;

public sealed record ViolationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ViolationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
