namespace Whycespace.Domain.BusinessSystem.Shared.Pricing;

// Shared basis for price adjustments (surcharge, discount-rule, markup).
// Flat is an absolute amount; Percentage is [0, 100].
public enum AdjustmentBasis
{
    Flat,
    Percentage
}
