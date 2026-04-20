namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Limit;

public sealed record LimitBreachedEvent(LimitId LimitId, int ObservedValue);
