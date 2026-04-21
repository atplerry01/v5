using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Validity;

public readonly record struct ValidityId
{
    public Guid Value { get; }

    public ValidityId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ValidityId cannot be empty.");
        Value = value;
    }
}
