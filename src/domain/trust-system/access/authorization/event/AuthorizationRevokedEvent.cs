namespace Whycespace.Domain.TrustSystem.Access.Authorization;

public sealed record AuthorizationRevokedEvent(
    AuthorizationId AuthorizationId);
