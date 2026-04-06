namespace Whycespace.Domain.EconomicSystem.Ledger.Obligation;

public sealed record ObligationStatus(string Value)
{
    public static readonly ObligationStatus Created = new("Created");
    public static readonly ObligationStatus Active = new("Active");
    public static readonly ObligationStatus Settled = new("Settled");
    public static readonly ObligationStatus Defaulted = new("Defaulted");

    public bool IsTerminal => this == Settled || this == Defaulted;
}
