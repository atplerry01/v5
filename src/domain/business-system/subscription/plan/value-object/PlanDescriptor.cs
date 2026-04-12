namespace Whycespace.Domain.BusinessSystem.Subscription.Plan;

public readonly record struct PlanDescriptor
{
    public string PlanName { get; }
    public string PlanTier { get; }

    public PlanDescriptor(string planName, string planTier)
    {
        if (string.IsNullOrWhiteSpace(planName))
            throw new ArgumentException("PlanName must not be empty.", nameof(planName));

        if (string.IsNullOrWhiteSpace(planTier))
            throw new ArgumentException("PlanTier must not be empty.", nameof(planTier));

        PlanName = planName;
        PlanTier = planTier;
    }
}
