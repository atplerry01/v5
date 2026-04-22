using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Observability.SystemSignal;

public sealed record SystemSignalDeprecatedEvent(
    SystemSignalId Id) : DomainEvent;
