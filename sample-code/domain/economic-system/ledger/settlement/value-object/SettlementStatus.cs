namespace Whycespace.Domain.EconomicSystem.Ledger.Settlement;

public sealed record SettlementStatus(string Value)
{
    public static readonly SettlementStatus Created = new("Created");
    public static readonly SettlementStatus Pending = new("Pending");
    public static readonly SettlementStatus Completed = new("Completed");

    public bool IsTerminal => this == Completed;

    public override string ToString() => Value;
}
