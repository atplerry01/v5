namespace Whycespace.Platform.Api.Business.Integration.Subscription;

public sealed record SubscriptionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record SubscriptionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
