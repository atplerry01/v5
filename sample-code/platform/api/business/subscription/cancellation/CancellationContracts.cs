namespace Whycespace.Platform.Api.Business.Subscription.Cancellation;

public sealed record CancellationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CancellationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
