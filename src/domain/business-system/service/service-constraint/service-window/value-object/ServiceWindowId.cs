using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceWindow;

public readonly record struct ServiceWindowId
{
    public Guid Value { get; }

    public ServiceWindowId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ServiceWindowId cannot be empty.");
        Value = value;
    }
}
