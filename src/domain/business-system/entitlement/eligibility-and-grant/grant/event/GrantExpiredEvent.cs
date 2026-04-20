namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

public sealed record GrantExpiredEvent(GrantId GrantId, DateTimeOffset ExpiredAt);
