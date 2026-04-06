namespace Whycespace.Engines.T2E.Business.Marketplace.Offer;

public record OfferCommand(
    string Action,
    string EntityId,
    object Payload
);
