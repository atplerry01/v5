namespace Whycespace.Platform.Api.Business.Subscription.Usage;

public sealed record UsageRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record UsageResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
