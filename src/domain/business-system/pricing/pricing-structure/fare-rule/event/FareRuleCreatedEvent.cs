namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.FareRule;

public sealed record FareRuleCreatedEvent(
    FareRuleId FareRuleId,
    TariffRef Tariff,
    FareRuleCode Code,
    FareRuleCondition Condition);
