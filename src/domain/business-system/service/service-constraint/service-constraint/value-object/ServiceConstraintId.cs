using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceConstraint;

public readonly record struct ServiceConstraintId
{
    public Guid Value { get; }

    public ServiceConstraintId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ServiceConstraintId cannot be empty.");
        Value = value;
    }
}
