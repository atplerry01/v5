using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Scheduler.Schedule;

public sealed record ScheduleCancelledEvent(
    Guid ScheduleId
) : DomainEvent;
