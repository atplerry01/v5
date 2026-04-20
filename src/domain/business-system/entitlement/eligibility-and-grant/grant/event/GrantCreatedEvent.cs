namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

public sealed record GrantCreatedEvent(
    GrantId GrantId,
    GrantSubjectRef Subject,
    GrantTargetRef Target,
    GrantScope Scope,
    GrantExpiry? Expiry);
