using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Settlement;

public readonly record struct SettlementId
{
    public Guid Value { get; }

    public SettlementId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SettlementId cannot be empty.");
        Value = value;
    }

    public static SettlementId From(Guid value) => new(value);
}
