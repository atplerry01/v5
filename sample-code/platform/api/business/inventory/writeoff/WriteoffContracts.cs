namespace Whycespace.Platform.Api.Business.Inventory.Writeoff;

public sealed record WriteoffRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record WriteoffResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
