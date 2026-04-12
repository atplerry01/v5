namespace Whycespace.Domain.BusinessSystem.Portfolio.Portfolio;

public sealed record PortfolioCreatedEvent(
    PortfolioId PortfolioId,
    PortfolioName PortfolioName);
