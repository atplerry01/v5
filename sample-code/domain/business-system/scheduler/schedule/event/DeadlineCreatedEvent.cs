using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Scheduler.Schedule;

public sealed record DeadlineCreatedEvent(
    Guid DeadlineId,
    Guid TargetEntityId,
    DateTimeOffset DueDate
) : DomainEvent;
