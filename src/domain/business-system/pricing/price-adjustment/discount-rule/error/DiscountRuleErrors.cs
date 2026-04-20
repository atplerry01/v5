namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.DiscountRule;

public static class DiscountRuleErrors
{
    public static DiscountRuleDomainException MissingId()
        => new("DiscountRuleId is required and must not be empty.");

    public static DiscountRuleDomainException InvalidStateTransition(DiscountRuleStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DiscountRuleDomainException PercentageOutOfRange(decimal value)
        => new($"DiscountAmount '{value}' is out of the [0, 100] range required for Percentage basis.");
}

public sealed class DiscountRuleDomainException : Exception
{
    public DiscountRuleDomainException(string message) : base(message) { }
}
