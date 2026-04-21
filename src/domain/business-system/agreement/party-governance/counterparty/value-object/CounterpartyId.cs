using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Counterparty;

public readonly record struct CounterpartyId
{
    public Guid Value { get; }

    public CounterpartyId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "CounterpartyId cannot be empty.");
        Value = value;
    }
}
