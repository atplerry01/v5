namespace Whycespace.Platform.Api.Business.Subscription.SubscriptionAccount;

public sealed record SubscriptionAccountRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record SubscriptionAccountResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
