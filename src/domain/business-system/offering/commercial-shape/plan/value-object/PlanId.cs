using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Plan;

public readonly record struct PlanId
{
    public Guid Value { get; }

    public PlanId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "PlanId cannot be empty.");
        Value = value;
    }
}
