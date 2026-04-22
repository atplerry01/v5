using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyDefinition;

public sealed record PolicyDeprecatedEvent(PolicyId Id) : DomainEvent;
