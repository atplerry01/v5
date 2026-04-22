using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.Principal;

public sealed record PrincipalRoleAssignedEvent(
    PrincipalId Id,
    string RoleId) : DomainEvent;
