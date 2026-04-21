using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Shared.Reference;

public readonly record struct CustomerRef
{
    public Guid Value { get; }

    public CustomerRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "CustomerRef cannot be empty.");
        Value = value;
    }
}
