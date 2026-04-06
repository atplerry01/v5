using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Scheduler.Schedule;

public sealed record DeadlineMissedEvent(
    Guid DeadlineId,
    Guid TargetEntityId,
    DateTimeOffset DueDate
) : DomainEvent;
