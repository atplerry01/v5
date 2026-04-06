namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public sealed record LedgerAccount
{
    public string Code { get; }
    public string Name { get; }

    public LedgerAccount(string code, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Code = code;
        Name = name;
    }

    public override string ToString() => $"{Code} — {Name}";
}
