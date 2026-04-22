using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.HumancapitalSanction;

public readonly record struct SanctionId
{
    public Guid Value { get; }

    public SanctionId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SanctionId cannot be empty.");
        Value = value;
    }
}
