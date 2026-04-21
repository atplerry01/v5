using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Financialcontrol.SpendControl;

public readonly record struct SpendControlId
{
    public Guid Value { get; }

    public SpendControlId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SpendControlId cannot be empty.");
        Value = value;
    }
}
