using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Observability.SystemSignal;

public sealed record SystemSignalDefinedEvent(
    SystemSignalId Id,
    string Name,
    SignalKind Kind,
    string Source) : DomainEvent;
