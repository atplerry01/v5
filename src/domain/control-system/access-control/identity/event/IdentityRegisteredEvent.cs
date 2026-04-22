using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.Identity;

public sealed record IdentityRegisteredEvent(
    IdentityId Id,
    string Name,
    IdentityKind Kind) : DomainEvent;
