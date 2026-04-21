using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Financialcontrol.VarianceControl;

public readonly record struct VarianceControlId
{
    public Guid Value { get; }

    public VarianceControlId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "VarianceControlId cannot be empty.");
        Value = value;
    }
}
