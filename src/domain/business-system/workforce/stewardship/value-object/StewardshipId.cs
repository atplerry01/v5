using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Workforce.Stewardship;

public readonly record struct StewardshipId
{
    public Guid Value { get; }

    public StewardshipId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "StewardshipId cannot be empty.");
        Value = value;
    }
}
