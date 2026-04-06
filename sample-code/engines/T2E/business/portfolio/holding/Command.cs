namespace Whycespace.Engines.T2E.Business.Portfolio.Holding;

public record HoldingCommand(
    string Action,
    string EntityId,
    object Payload
);
