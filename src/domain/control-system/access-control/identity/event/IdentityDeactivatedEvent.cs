using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.Identity;

public sealed record IdentityDeactivatedEvent(
    IdentityId Id) : DomainEvent;
