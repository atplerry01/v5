namespace Whycespace.Domain.BusinessSystem.Scheduler.Slot;

public static class SlotErrors
{
    public static SlotDomainException MissingId()
        => new("SlotId is required and must not be empty.");

    public static SlotDomainException InvalidTimeSlot()
        => new("Slot must have valid duration (end after start).");

    public static SlotDomainException AlreadyCancelled(SlotId id)
        => new($"Slot '{id.Value}' has already been cancelled.");

    public static SlotDomainException InvalidStateTransition(SlotStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class SlotDomainException : Exception
{
    public SlotDomainException(string message) : base(message) { }
}
