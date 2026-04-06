namespace Whycespace.Platform.Api.Business.Inventory.Transfer;

public sealed record TransferRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record TransferResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
