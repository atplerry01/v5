using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Renewal;

public readonly record struct RenewalSourceId
{
    public Guid Value { get; }

    public RenewalSourceId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "RenewalSourceId cannot be empty.");
        Value = value;
    }
}
