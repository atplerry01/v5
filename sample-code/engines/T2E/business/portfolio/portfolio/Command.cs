namespace Whycespace.Engines.T2E.Business.Portfolio.Portfolio;

public record PortfolioCommand(
    string Action,
    string EntityId,
    object Payload
);
