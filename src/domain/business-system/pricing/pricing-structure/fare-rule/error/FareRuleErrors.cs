namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.FareRule;

public static class FareRuleErrors
{
    public static FareRuleDomainException MissingId()
        => new("FareRuleId is required and must not be empty.");

    public static FareRuleDomainException MissingTariffRef()
        => new("FareRule must reference a tariff.");

    public static FareRuleDomainException InvalidStateTransition(FareRuleStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class FareRuleDomainException : Exception
{
    public FareRuleDomainException(string message) : base(message) { }
}
