using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.Contract;

public readonly record struct ContractId
{
    public Guid Value { get; }

    public ContractId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ContractId cannot be empty.");
        Value = value;
    }
}
