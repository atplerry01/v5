namespace Whycespace.Domain.BusinessSystem.Scheduler.Schedule;

public sealed class ScheduleSpecification
{
    public bool IsSatisfiedBy(ScheduleAggregate schedule)
    {
        return schedule.Id != default
            && Enum.IsDefined(schedule.Status);
    }
}
