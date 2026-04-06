namespace Whycespace.Engines.T2E.Business.Integration.Contract;

public record ContractCommand(
    string Action,
    string EntityId,
    object Payload
);
