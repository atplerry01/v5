namespace Whycespace.Platform.Api.Business.Subscription.Plan;

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
