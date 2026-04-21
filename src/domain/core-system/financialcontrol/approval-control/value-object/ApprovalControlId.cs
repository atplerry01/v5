using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Financialcontrol.ApprovalControl;

public readonly record struct ApprovalControlId
{
    public Guid Value { get; }

    public ApprovalControlId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ApprovalControlId cannot be empty.");
        Value = value;
    }
}
