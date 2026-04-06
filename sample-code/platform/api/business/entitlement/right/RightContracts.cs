namespace Whycespace.Platform.Api.Business.Entitlement.Right;

public sealed record RightRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RightResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
