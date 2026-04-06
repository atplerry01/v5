namespace Whycespace.Engines.T2E.Business.Scheduler.Schedule;

public record ScheduleCommand(
    string Action,
    string EntityId,
    object Payload
);
