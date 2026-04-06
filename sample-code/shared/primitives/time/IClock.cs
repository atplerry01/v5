namespace Whycespace.Shared.Primitives.Time;

public interface IClock
{
    DateTime UtcNow { get; }
    DateTimeOffset UtcNowOffset => new(UtcNow, TimeSpan.Zero);
}

public sealed class SystemClock : IClock
{
    public static readonly SystemClock Instance = new();

    public DateTime UtcNow => DateTime.UtcNow;
    public DateTimeOffset UtcNowOffset => DateTimeOffset.UtcNow;
}

/// <summary>
/// Deterministic clock for replay and testing.
/// Time only advances when explicitly set.
/// </summary>
public sealed class DeterministicClock : IClock
{
    private DateTimeOffset _current;

    public DeterministicClock(DateTimeOffset initial)
    {
        _current = initial;
    }

    public DateTime UtcNow => _current.UtcDateTime;
    public DateTimeOffset UtcNowOffset => _current;

    public void Advance(TimeSpan duration) => _current = _current.Add(duration);
    public void Set(DateTimeOffset value) => _current = value;
}
