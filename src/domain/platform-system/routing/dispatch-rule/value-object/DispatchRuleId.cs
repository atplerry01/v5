using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Routing.DispatchRule;

public readonly record struct DispatchRuleId
{
    public Guid Value { get; }

    public DispatchRuleId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "DispatchRuleId cannot be empty.");
        Value = value;
    }
}
