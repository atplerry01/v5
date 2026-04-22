using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.AccessPolicy;

public sealed record AccessPolicyActivatedEvent(
    AccessPolicyId Id) : DomainEvent;
