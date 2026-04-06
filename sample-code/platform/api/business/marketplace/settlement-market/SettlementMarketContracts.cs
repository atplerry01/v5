namespace Whycespace.Platform.Api.Business.Marketplace.SettlementMarket;

public sealed record SettlementMarketRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record SettlementMarketResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
