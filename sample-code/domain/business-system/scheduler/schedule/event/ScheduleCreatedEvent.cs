using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Scheduler.Schedule;

public sealed record ScheduleCreatedEvent(
    Guid ScheduleId,
    string CronExpression,
    string TimeZone,
    DateTimeOffset StartTime,
    DateTimeOffset? EndTime
) : DomainEvent;
