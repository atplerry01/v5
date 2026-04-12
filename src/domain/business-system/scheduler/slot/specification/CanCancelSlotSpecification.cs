namespace Whycespace.Domain.BusinessSystem.Scheduler.Slot;

public sealed class CanCancelSlotSpecification
{
    public bool IsSatisfiedBy(SlotStatus status)
    {
        return status == SlotStatus.Open;
    }
}
