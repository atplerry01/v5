namespace Whycespace.Platform.Api.Business.Inventory.Movement;

public sealed record MovementRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record MovementResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
