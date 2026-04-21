using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Limit;

public readonly record struct LimitSubjectId
{
    public Guid Value { get; }

    public LimitSubjectId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "LimitSubjectId cannot be empty.");
        Value = value;
    }
}
