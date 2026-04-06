namespace Whycespace.Engines.T2E.Business.Execution.Charge;

public record ChargeCommand(
    string Action,
    string EntityId,
    object Payload
);
