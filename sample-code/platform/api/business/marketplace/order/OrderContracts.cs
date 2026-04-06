namespace Whycespace.Platform.Api.Business.Marketplace.Order;

public sealed record OrderRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record OrderResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
