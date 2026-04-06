namespace Whycespace.Platform.Api.Economic.Ledger.Treasury;

public sealed record TreasuryRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record TreasuryResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
