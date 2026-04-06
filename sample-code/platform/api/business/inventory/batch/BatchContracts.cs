namespace Whycespace.Platform.Api.Business.Inventory.Batch;

public sealed record BatchRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record BatchResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
