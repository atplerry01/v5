namespace Whycespace.Engines.T2E.Economic.Ledger.Treasury;

public record TreasuryCommand(string Action, string EntityId, object Payload);
public sealed record CreateTreasuryCommand(string Id) : TreasuryCommand("Create", Id, null!);
