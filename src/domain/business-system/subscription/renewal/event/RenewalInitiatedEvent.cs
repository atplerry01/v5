namespace Whycespace.Domain.BusinessSystem.Subscription.Renewal;

public sealed record RenewalInitiatedEvent(RenewalId RenewalId, RenewalRequest Request);
