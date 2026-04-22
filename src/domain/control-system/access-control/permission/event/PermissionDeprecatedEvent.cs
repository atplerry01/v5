using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.Permission;

public sealed record PermissionDeprecatedEvent(PermissionId Id) : DomainEvent;
