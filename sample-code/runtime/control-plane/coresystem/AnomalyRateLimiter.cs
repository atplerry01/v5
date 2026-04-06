namespace Whycespace.Runtime.ControlPlane.CoreSystem;

/// <summary>
/// Rate limiter for anomaly emission. Prevents cascading alert storms
/// by enforcing a maximum number of anomalies per time window.
///
/// Uses a sliding window counter. Thread-safe via Interlocked operations.
/// </summary>
public sealed class AnomalyRateLimiter
{
    private readonly int _maxPerWindow;
    private readonly long _windowMs;
    private long _windowStart;
    private int _count;

    public AnomalyRateLimiter(int maxPerWindow, TimeSpan window)
    {
        _maxPerWindow = maxPerWindow > 0 ? maxPerWindow : 100;
        _windowMs = (long)window.TotalMilliseconds;
        _windowStart = Environment.TickCount64;
        _count = 0;
    }

    /// <summary>
    /// Returns true if the anomaly is within rate limits and should be emitted.
    /// Returns false if the rate limit has been exceeded for this window.
    /// </summary>
    public bool TryAcquire()
    {
        var now = Environment.TickCount64;

        // Reset window if expired
        if (now - Interlocked.Read(ref _windowStart) >= _windowMs)
        {
            Interlocked.Exchange(ref _windowStart, now);
            Interlocked.Exchange(ref _count, 0);
        }

        var current = Interlocked.Increment(ref _count);
        return current <= _maxPerWindow;
    }

    public int CurrentCount => Volatile.Read(ref _count);
    public bool IsThrottled => CurrentCount >= _maxPerWindow;
}
