using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceDefinition;

public readonly record struct ServiceDefinitionId
{
    public Guid Value { get; }

    public ServiceDefinitionId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ServiceDefinitionId cannot be empty.");
        Value = value;
    }
}
