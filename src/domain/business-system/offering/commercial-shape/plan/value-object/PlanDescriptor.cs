using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Plan;

public readonly record struct PlanDescriptor
{
    public string PlanName { get; }
    public string PlanTier { get; }

    public PlanDescriptor(string planName, string planTier)
    {
        Guard.Against(string.IsNullOrWhiteSpace(planName), "PlanName must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(planTier), "PlanTier must not be empty.");

        PlanName = planName!;
        PlanTier = planTier!;
    }
}
