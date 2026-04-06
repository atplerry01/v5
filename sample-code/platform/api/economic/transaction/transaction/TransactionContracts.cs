namespace Whycespace.Platform.Api.Economic.Transaction.Transaction;

public sealed record TransactionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record TransactionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
