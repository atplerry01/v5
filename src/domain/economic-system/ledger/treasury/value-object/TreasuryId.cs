using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Treasury;

public readonly record struct TreasuryId
{
    public Guid Value { get; }

    public TreasuryId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "TreasuryId cannot be empty.");
        Value = value;
    }

    public static TreasuryId From(Guid value) => new(value);
}
