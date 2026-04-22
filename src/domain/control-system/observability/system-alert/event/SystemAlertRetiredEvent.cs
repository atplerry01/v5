using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Observability.SystemAlert;

public sealed record SystemAlertRetiredEvent(SystemAlertId Id) : DomainEvent;
