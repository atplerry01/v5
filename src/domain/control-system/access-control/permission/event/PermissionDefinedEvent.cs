using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.Permission;

public sealed record PermissionDefinedEvent(PermissionId Id, string Name, string ResourceScope, ActionMask Actions) : DomainEvent;
