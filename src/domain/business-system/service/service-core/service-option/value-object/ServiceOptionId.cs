using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceOption;

public readonly record struct ServiceOptionId
{
    public Guid Value { get; }

    public ServiceOptionId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ServiceOptionId cannot be empty.");
        Value = value;
    }
}
