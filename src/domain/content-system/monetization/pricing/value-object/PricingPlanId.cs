namespace Whycespace.Domain.ContentSystem.Monetization.Pricing;

public readonly record struct PricingPlanId(Guid Value)
{
    public static PricingPlanId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
