namespace Whycespace.Platform.Api.Constitutional.Policy.Constraint;

public sealed record ConstraintRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ConstraintResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
