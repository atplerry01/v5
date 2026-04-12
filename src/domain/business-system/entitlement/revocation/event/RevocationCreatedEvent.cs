namespace Whycespace.Domain.BusinessSystem.Entitlement.Revocation;

public sealed record RevocationCreatedEvent(RevocationId RevocationId, RevocationTargetId TargetId);
