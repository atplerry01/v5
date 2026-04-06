namespace Whycespace.Platform.Api.Business.Inventory.Item;

public sealed record ItemRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ItemResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
