namespace Whycespace.Domain.TrustSystem.Access.Authorization;

public sealed record AuthorizationDeniedEvent(
    AuthorizationId AuthorizationId,
    AuthorizationScope Scope);
