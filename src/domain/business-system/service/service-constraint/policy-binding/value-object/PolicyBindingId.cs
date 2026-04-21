using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.PolicyBinding;

public readonly record struct PolicyBindingId
{
    public Guid Value { get; }

    public PolicyBindingId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "PolicyBindingId cannot be empty.");
        Value = value;
    }
}
