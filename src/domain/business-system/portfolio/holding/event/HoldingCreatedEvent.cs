namespace Whycespace.Domain.BusinessSystem.Portfolio.Holding;

public sealed record HoldingCreatedEvent(
    HoldingId HoldingId,
    PortfolioReference PortfolioReference,
    AssetReference AssetReference,
    HoldingQuantity Quantity);
