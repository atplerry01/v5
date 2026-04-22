using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.Identity;

public sealed record IdentitySuspendedEvent(
    IdentityId Id,
    string Reason) : DomainEvent;
