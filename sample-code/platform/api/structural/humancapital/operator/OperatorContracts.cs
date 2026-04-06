namespace Whycespace.Platform.Api.Structural.Humancapital.Operator;

public sealed record OperatorRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record OperatorResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
