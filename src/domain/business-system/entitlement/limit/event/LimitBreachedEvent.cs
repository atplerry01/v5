namespace Whycespace.Domain.BusinessSystem.Entitlement.Limit;

public sealed record LimitBreachedEvent(LimitId LimitId, int ObservedValue);
