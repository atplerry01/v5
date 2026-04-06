namespace Whycespace.Engines.T2E.Business.Agreement.Contract;

public record ContractCommand(
    string Action,
    string EntityId,
    object Payload
);
