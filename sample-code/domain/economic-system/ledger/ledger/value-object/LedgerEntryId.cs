using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public sealed record LedgerEntryId
{
    public Guid Value { get; }

    public LedgerEntryId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LedgerEntryId cannot be empty.", nameof(value));

        Value = value;
    }

    public static LedgerEntryId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));

    public override string ToString() => Value.ToString();
}
