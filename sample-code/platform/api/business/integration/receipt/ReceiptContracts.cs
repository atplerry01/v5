namespace Whycespace.Platform.Api.Business.Integration.Receipt;

public sealed record ReceiptRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ReceiptResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
