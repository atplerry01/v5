namespace Whycespace.Domain.TrustSystem.Access.Permission;

public sealed record PermissionDefinedEvent(
    Guid PermissionId,
    string Name,
    string Resource,
    string Action,
    string Scope) : DomainEvent;

public sealed record PermissionRevokedEvent(
    Guid PermissionId,
    string Name) : DomainEvent;
