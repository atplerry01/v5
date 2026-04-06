namespace Whycespace.Engines.T2E.Constitutional.Chain.Ledger;

public record ChainLedgerCommand(
    string Action,
    string EntityId,
    object Payload
);
