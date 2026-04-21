using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Clause;

public readonly record struct ClauseId
{
    public Guid Value { get; }

    public ClauseId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ClauseId cannot be empty.");
        Value = value;
    }
}
