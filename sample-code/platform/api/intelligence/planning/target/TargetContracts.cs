namespace Whycespace.Platform.Api.Intelligence.Planning.Target;

public sealed record TargetRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record TargetResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
