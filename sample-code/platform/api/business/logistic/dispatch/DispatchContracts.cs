namespace Whycespace.Platform.Api.Business.Logistic.Dispatch;

public sealed record DispatchRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record DispatchResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
