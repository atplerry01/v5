namespace Whycespace.Platform.Api.Business.Entitlement.UsageRight;

public sealed record UsageRightRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record UsageRightResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
