using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Amendment;

public readonly record struct AmendmentTargetId
{
    public Guid Value { get; }

    public AmendmentTargetId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "AmendmentTargetId cannot be empty.");
        Value = value;
    }
}
