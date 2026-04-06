namespace Whycespace.Platform.Api.Structural.Humancapital.Assignment;

public sealed record AssignmentRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AssignmentResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
