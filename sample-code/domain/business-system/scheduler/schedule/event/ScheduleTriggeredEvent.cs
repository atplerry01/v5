using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Scheduler.Schedule;

public sealed record ScheduleTriggeredEvent(
    Guid ScheduleId,
    DateTimeOffset TriggeredAt
) : DomainEvent;
