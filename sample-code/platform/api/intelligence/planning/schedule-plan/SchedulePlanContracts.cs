namespace Whycespace.Platform.Api.Intelligence.Planning.SchedulePlan;

public sealed record SchedulePlanRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record SchedulePlanResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
