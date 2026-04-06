namespace Whycespace.Engines.T2E.Business.Marketplace.Bid;

public record BidCommand(
    string Action,
    string EntityId,
    object Payload
);
