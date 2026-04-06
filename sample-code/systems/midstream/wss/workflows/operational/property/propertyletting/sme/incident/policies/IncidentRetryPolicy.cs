namespace Whycespace.Systems.Midstream.Wss.Workflows.Operational.Property.PropertyLetting.Sme.Incident;

public sealed class IncidentRetryPolicy
{
    public int MaxAttempts { get; } = 3;
    private readonly TimeSpan _baseDelay = TimeSpan.FromSeconds(1);

    public bool ShouldRetry(int attempt, Exception exception)
    {
        if (attempt >= MaxAttempts)
            return false;

        if (exception is InvalidOperationException or ArgumentException)
            return false;

        return true;
    }

    public TimeSpan GetDelay(int attempt)
    {
        var multiplier = Math.Pow(2, attempt - 1);
        return TimeSpan.FromTicks((long)(_baseDelay.Ticks * multiplier));
    }
}
