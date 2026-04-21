using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Contract;

public readonly record struct PartyId
{
    public Guid Value { get; }

    public PartyId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "PartyId cannot be empty.");
        Value = value;
    }
}
