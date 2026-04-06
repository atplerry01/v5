namespace Whycespace.Platform.Api.Business.Inventory.Stock;

public sealed record StockRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record StockResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
