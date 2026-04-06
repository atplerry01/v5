namespace Whycespace.Domain.TrustSystem.Access.Role;

public sealed record RoleCreatedEvent(
    Guid RoleId,
    string Name,
    string ClusterId) : DomainEvent;

public sealed record RolePermissionAssignedEvent(
    Guid RoleId,
    Guid PermissionId) : DomainEvent;

public sealed record RolePermissionRemovedEvent(
    Guid RoleId,
    Guid PermissionId) : DomainEvent;

public sealed record RoleDeactivatedEvent(
    Guid RoleId,
    string Name) : DomainEvent;

public sealed record RoleAssignedEvent(
    Guid RoleId,
    Guid IdentityId,
    string ClusterId) : DomainEvent;
