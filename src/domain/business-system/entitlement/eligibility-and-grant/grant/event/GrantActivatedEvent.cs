namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

public sealed record GrantActivatedEvent(GrantId GrantId, DateTimeOffset ActivatedAt);
