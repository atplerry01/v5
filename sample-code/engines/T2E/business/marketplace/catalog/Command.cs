namespace Whycespace.Engines.T2E.Business.Marketplace.Catalog;

public record CatalogCommand(
    string Action,
    string EntityId,
    object Payload
);
