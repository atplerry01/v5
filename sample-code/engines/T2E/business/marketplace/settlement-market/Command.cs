namespace Whycespace.Engines.T2E.Business.Marketplace.SettlementMarket;

public record SettlementMarketCommand(
    string Action,
    string EntityId,
    object Payload
);
