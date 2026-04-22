namespace Whycespace.Domain.TrustSystem.Access.Authorization;

public sealed record AuthorizationGrantedEvent(
    AuthorizationId AuthorizationId,
    AuthorizationScope Scope);
