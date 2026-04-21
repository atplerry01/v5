using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Limit;

public readonly record struct LimitId
{
    public Guid Value { get; }

    public LimitId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "LimitId cannot be empty.");
        Value = value;
    }
}
