namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

public sealed record GrantRevokedEvent(GrantId GrantId, DateTimeOffset RevokedAt);
