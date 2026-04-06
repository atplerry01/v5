namespace Whycespace.Engines.T2E.Business.Scheduler.Booking;

public record BookingCommand(
    string Action,
    string EntityId,
    object Payload
);
