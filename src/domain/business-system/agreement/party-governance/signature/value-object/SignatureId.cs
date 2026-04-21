using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Signature;

public readonly record struct SignatureId
{
    public Guid Value { get; }

    public SignatureId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SignatureId cannot be empty.");
        Value = value;
    }
}
