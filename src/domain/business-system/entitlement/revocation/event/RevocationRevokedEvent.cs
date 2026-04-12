namespace Whycespace.Domain.BusinessSystem.Entitlement.Revocation;

public sealed record RevocationRevokedEvent(RevocationId RevocationId, string Reason);
