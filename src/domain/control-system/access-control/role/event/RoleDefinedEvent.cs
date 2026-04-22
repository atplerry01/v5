using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.Role;

public sealed record RoleDefinedEvent(RoleId Id, string Name, IReadOnlySet<string> PermissionIds, string? ParentRoleId) : DomainEvent;
