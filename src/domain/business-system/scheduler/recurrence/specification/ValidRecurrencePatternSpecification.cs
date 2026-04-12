namespace Whycespace.Domain.BusinessSystem.Scheduler.Recurrence;

public sealed class ValidRecurrencePatternSpecification
{
    public bool IsSatisfiedBy(RecurrencePattern pattern)
    {
        if (!Enum.IsDefined(pattern.Frequency))
            return false;

        if (pattern.Interval <= 0)
            return false;

        if (pattern.EndTicks.HasValue && pattern.EndTicks.Value <= pattern.StartTicks)
            return false;

        if (pattern.MaxOccurrences.HasValue && pattern.MaxOccurrences.Value <= 0)
            return false;

        if (!pattern.EndTicks.HasValue && !pattern.MaxOccurrences.HasValue)
            return false;

        return true;
    }
}
