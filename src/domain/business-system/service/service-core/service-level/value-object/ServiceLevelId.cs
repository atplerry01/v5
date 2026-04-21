using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;

public readonly record struct ServiceLevelId
{
    public Guid Value { get; }

    public ServiceLevelId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ServiceLevelId cannot be empty.");
        Value = value;
    }
}
