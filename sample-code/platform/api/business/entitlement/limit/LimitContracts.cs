namespace Whycespace.Platform.Api.Business.Entitlement.Limit;

public sealed record LimitRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record LimitResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
