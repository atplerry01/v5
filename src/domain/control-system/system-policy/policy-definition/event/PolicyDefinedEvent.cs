using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyDefinition;

public sealed record PolicyDefinedEvent(
    PolicyId Id,
    string Name,
    PolicyScope Scope,
    int Version) : DomainEvent;
