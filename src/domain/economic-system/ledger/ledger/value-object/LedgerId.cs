using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public readonly record struct LedgerId
{
    public Guid Value { get; }

    public LedgerId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "LedgerId cannot be empty.");
        Value = value;
    }

    public static LedgerId From(Guid value) => new(value);
}
