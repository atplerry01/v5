using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Scheduling.SystemJob;

public sealed record SystemJobDeprecatedEvent(SystemJobId Id) : DomainEvent;
