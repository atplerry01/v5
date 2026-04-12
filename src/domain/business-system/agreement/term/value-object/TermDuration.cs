namespace Whycespace.Domain.BusinessSystem.Agreement.Term;

public readonly record struct TermDuration
{
    public int DurationInDays { get; }

    public TermDuration(int durationInDays)
    {
        if (durationInDays <= 0)
            throw new ArgumentException("Duration must be greater than zero days.", nameof(durationInDays));

        DurationInDays = durationInDays;
    }
}
