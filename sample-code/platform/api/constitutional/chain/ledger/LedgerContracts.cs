namespace Whycespace.Platform.Api.Constitutional.Chain.Ledger;

public sealed record LedgerRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record LedgerResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
