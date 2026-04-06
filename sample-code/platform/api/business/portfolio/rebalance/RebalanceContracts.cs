namespace Whycespace.Platform.Api.Business.Portfolio.Rebalance;

public sealed record RebalanceRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RebalanceResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
