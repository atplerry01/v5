using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Obligation;

public readonly record struct ObligationId
{
    public Guid Value { get; }

    public ObligationId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ObligationId cannot be empty.");
        Value = value;
    }

    public static ObligationId From(Guid value) => new(value);
}
