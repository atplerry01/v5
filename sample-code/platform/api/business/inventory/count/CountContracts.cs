namespace Whycespace.Platform.Api.Business.Inventory.Count;

public sealed record CountRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CountResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
