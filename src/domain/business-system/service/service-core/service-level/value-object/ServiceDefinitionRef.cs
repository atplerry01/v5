using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;

public readonly record struct ServiceDefinitionRef
{
    public Guid Value { get; }

    public ServiceDefinitionRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ServiceDefinitionRef cannot be empty.");
        Value = value;
    }
}
