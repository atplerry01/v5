namespace Whycespace.Domain.TrustSystem.Access.Authorization;

public sealed record AuthorizationGrantedEvent(
    AuthorizationId AuthorizationId,
    AuthorizationScope Scope);

public sealed record AuthorizationDeniedEvent(
    AuthorizationId AuthorizationId,
    AuthorizationScope Scope);

public sealed record AuthorizationRevokedEvent(
    AuthorizationId AuthorizationId);
