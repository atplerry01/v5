namespace Whycespace.Platform.Api.Economic.Revenue.Revenue;

public sealed record RevenueRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RevenueResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
