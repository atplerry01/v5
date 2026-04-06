namespace Whycespace.Platform.Api.Economic.Ledger.Settlement;

public sealed record SettlementRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record SettlementResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
