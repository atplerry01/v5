namespace Whycespace.Engines.T2E.Business.Scheduler.Calendar;

public record CalendarCommand(
    string Action,
    string EntityId,
    object Payload
);
