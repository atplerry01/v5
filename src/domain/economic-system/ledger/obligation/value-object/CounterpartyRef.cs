using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Obligation;

public readonly record struct CounterpartyRef
{
    public Guid Value { get; }

    public CounterpartyRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "CounterpartyRef cannot be empty.");
        Value = value;
    }
}
