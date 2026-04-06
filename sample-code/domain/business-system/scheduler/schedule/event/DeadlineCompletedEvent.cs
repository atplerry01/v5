using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Scheduler.Schedule;

public sealed record DeadlineCompletedEvent(
    Guid DeadlineId,
    Guid TargetEntityId
) : DomainEvent;
