namespace Whycespace.Domain.BusinessSystem.Subscription.Plan;

public readonly record struct PlanId
{
    public Guid Value { get; }

    public PlanId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PlanId cannot be empty.", nameof(value));

        Value = value;
    }
}
