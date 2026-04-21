using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Financialcontrol.GlobalInvariant;

public readonly record struct GlobalInvariantId
{
    public Guid Value { get; }

    public GlobalInvariantId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "GlobalInvariantId cannot be empty.");
        Value = value;
    }
}
