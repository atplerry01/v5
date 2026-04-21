using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Financialcontrol.ReserveControl;

public readonly record struct ReserveControlId
{
    public Guid Value { get; }

    public ReserveControlId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ReserveControlId cannot be empty.");
        Value = value;
    }
}
