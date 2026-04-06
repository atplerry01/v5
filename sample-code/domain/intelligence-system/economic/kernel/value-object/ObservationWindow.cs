namespace Whycespace.Domain.IntelligenceSystem.Economic;

public sealed record ObservationWindow
{
    public DateTimeOffset ObservedAt { get; }
    public DateTimeOffset WindowStart { get; }
    public DateTimeOffset WindowEnd { get; }

    public ObservationWindow(DateTimeOffset observedAt, DateTimeOffset windowStart, DateTimeOffset windowEnd)
    {
        if (windowStart > windowEnd)
            throw new ArgumentException("WindowStart must be before or equal to WindowEnd.");
        ObservedAt = observedAt;
        WindowStart = windowStart;
        WindowEnd = windowEnd;
    }
}
