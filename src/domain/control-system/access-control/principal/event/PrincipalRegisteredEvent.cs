using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.Principal;

public sealed record PrincipalRegisteredEvent(
    PrincipalId Id,
    string Name,
    PrincipalKind Kind,
    string IdentityId) : DomainEvent;
