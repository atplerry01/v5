using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Temporal.ScheduleReference;

public sealed record ScheduleReferenceCreatedEvent(Guid ScheduleId, string ScheduleName, string CronExpression) : DomainEvent;
public sealed record ScheduleReferenceDisabledEvent(Guid ScheduleId) : DomainEvent;
