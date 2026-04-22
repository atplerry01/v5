using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Scheduling.ScheduleControl;

public sealed record ScheduleControlDefinedEvent(ScheduleControlId Id, string JobDefinitionId, string TriggerExpression) : DomainEvent;
