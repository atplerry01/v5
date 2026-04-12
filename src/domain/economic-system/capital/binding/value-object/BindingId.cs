using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Binding;

public readonly record struct BindingId
{
    public Guid Value { get; }

    public BindingId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "BindingId cannot be empty.");
        Value = value;
    }
}
