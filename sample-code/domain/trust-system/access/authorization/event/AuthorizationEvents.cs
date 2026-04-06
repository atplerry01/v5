namespace Whycespace.Domain.TrustSystem.Access.Authorization;

public sealed record AuthorizationEvaluatedEvent(
    Guid AuthorizationId,
    Guid IdentityId,
    string Resource,
    string Action,
    string PolicyId) : DomainEvent;

public sealed record AuthorizationApprovedEvent(
    Guid AuthorizationId,
    Guid IdentityId,
    string Resource,
    string Action) : DomainEvent;

public sealed record AuthorizationDeniedEvent(
    Guid AuthorizationId,
    Guid IdentityId,
    string Resource,
    string Action,
    string Reason) : DomainEvent;
