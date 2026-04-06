namespace Whycespace.Platform.Api.Business.Integration.Retry;

public sealed record RetryRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RetryResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
