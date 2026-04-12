namespace Whycespace.Domain.BusinessSystem.Scheduler.Slot;

public sealed class SlotSpecification
{
    public bool IsSatisfiedBy(SlotAggregate slot)
    {
        return slot.Id != default
            && slot.TimeSlot.EndTicks > slot.TimeSlot.StartTicks
            && Enum.IsDefined(slot.Status);
    }
}
