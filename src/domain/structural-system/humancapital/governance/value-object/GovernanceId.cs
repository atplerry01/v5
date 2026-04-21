using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Governance;

public readonly record struct GovernanceId
{
    public Guid Value { get; }

    public GovernanceId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "GovernanceId cannot be empty.");
        Value = value;
    }
}
