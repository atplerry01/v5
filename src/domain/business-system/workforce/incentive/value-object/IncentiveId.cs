using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Workforce.Incentive;

public readonly record struct IncentiveId
{
    public Guid Value { get; }

    public IncentiveId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "IncentiveId cannot be empty.");
        Value = value;
    }
}
