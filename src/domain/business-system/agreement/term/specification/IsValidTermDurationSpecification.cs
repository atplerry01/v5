namespace Whycespace.Domain.BusinessSystem.Agreement.Term;

public sealed class IsValidTermDurationSpecification
{
    public bool IsSatisfiedBy(TermDuration duration)
    {
        return duration.DurationInDays > 0;
    }
}
