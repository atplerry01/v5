using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Eligibility;

public readonly record struct EligibilityId
{
    public Guid Value { get; }

    public EligibilityId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "EligibilityId cannot be empty.");
        Value = value;
    }
}
