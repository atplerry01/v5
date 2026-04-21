using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Approval;

public readonly record struct ApprovalId
{
    public Guid Value { get; }

    public ApprovalId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ApprovalId cannot be empty.");
        Value = value;
    }
}
