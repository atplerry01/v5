namespace Whycespace.Engines.T2E.Business.Agreement.Counterparty;

public record CounterpartyCommand(
    string Action,
    string EntityId,
    object Payload
);
