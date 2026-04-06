namespace Whycespace.Platform.Api.Business.Inventory.Sku;

public sealed record SkuRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record SkuResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
