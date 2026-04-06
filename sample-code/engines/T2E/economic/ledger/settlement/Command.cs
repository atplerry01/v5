using Whycespace.Shared.Primitives.Money;

namespace Whycespace.Engines.T2E.Economic.Ledger.Settlement;

public record SettlementCommand(string Action, string EntityId, object Payload);
public sealed record CreateSettlementCommand(string Id, string PayeeIdentityId, Money Amount) : SettlementCommand("Create", Id, null!);
