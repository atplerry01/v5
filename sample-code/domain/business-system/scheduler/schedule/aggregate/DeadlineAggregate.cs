using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Scheduler.Schedule;

public sealed class DeadlineAggregate : AggregateRoot
{
    public Guid TargetEntityId { get; private set; }
    public DateTimeOffset DueDate { get; private set; }
    public DeadlineStatus Status { get; private set; } = DeadlineStatus.Pending;

    public static DeadlineAggregate Create(Guid deadlineId, Guid targetEntityId, DateTimeOffset dueDate, DateTimeOffset now)
    {
        if (dueDate <= now)
            throw new DomainException(DeadlineErrors.InvalidDueDate, "Due date must be in the future.");

        var deadline = new DeadlineAggregate();
        deadline.Apply(new DeadlineCreatedEvent(deadlineId, targetEntityId, dueDate));
        return deadline;
    }

    public void MarkCompleted()
    {
        if (Status == DeadlineStatus.Completed)
            throw new DomainException(DeadlineErrors.AlreadyCompleted, "Deadline is already completed.");

        if (Status.IsTerminal)
            throw new DomainException(DeadlineErrors.InvalidTransition, $"Cannot complete deadline in '{Status.Value}' status.");

        Apply(new DeadlineCompletedEvent(Id, TargetEntityId));
    }

    public void MarkMissed()
    {
        if (Status == DeadlineStatus.Missed)
            throw new DomainException(DeadlineErrors.AlreadyMissed, "Deadline is already missed.");

        if (Status.IsTerminal)
            throw new DomainException(DeadlineErrors.InvalidTransition, $"Cannot mark deadline missed in '{Status.Value}' status.");

        Apply(new DeadlineMissedEvent(Id, TargetEntityId, DueDate));
    }

    private void Apply(DeadlineCreatedEvent e)
    {
        Id = e.DeadlineId;
        TargetEntityId = e.TargetEntityId;
        DueDate = e.DueDate;
        Status = DeadlineStatus.Pending;
        RaiseDomainEvent(e);
    }

    private void Apply(DeadlineCompletedEvent e)
    {
        Status = DeadlineStatus.Completed;
        RaiseDomainEvent(e);
    }

    private void Apply(DeadlineMissedEvent e)
    {
        Status = DeadlineStatus.Missed;
        RaiseDomainEvent(e);
    }
}
