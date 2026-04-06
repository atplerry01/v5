namespace Whycespace.Engines.T2E.Business.Scheduler.Slot;

public record SlotCommand(
    string Action,
    string EntityId,
    object Payload
);
