namespace Whycespace.Platform.Api.Intelligence.Planning.CapacityPlan;

public sealed record CapacityPlanRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CapacityPlanResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
