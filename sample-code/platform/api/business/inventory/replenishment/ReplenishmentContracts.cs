namespace Whycespace.Platform.Api.Business.Inventory.Replenishment;

public sealed record ReplenishmentRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ReplenishmentResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
