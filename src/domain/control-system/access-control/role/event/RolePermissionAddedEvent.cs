using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.Role;

public sealed record RolePermissionAddedEvent(RoleId Id, string PermissionId) : DomainEvent;
