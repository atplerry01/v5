using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Renewal;

public readonly record struct RenewalId
{
    public Guid Value { get; }

    public RenewalId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "RenewalId cannot be empty.");
        Value = value;
    }
}
