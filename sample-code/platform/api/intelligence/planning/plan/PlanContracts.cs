namespace Whycespace.Platform.Api.Intelligence.Planning.Plan;

public sealed record PlanRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record PlanResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
