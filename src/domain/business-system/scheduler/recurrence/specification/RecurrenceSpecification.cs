namespace Whycespace.Domain.BusinessSystem.Scheduler.Recurrence;

public sealed class RecurrenceSpecification
{
    public bool IsSatisfiedBy(RecurrenceAggregate recurrence)
    {
        return recurrence.Id != default
            && Enum.IsDefined(recurrence.Pattern.Frequency)
            && recurrence.Pattern.Interval > 0
            && (recurrence.Pattern.EndTicks.HasValue || recurrence.Pattern.MaxOccurrences.HasValue)
            && Enum.IsDefined(recurrence.Status);
    }
}
