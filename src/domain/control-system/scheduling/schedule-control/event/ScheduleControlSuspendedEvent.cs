using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Scheduling.ScheduleControl;

public sealed record ScheduleControlSuspendedEvent(ScheduleControlId Id) : DomainEvent;
