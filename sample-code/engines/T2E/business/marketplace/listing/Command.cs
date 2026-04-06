namespace Whycespace.Engines.T2E.Business.Marketplace.Listing;

public record ListingCommand(
    string Action,
    string EntityId,
    object Payload
);
