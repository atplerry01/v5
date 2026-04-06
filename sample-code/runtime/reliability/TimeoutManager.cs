using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Reliability;

public sealed class TimeoutManager
{
    private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);
    private readonly Dictionary<string, TimeSpan> _timeouts = new();
    private readonly IClock _clock;

    public TimeoutManager(IClock? clock = null)
    {
        _clock = clock ?? SystemClock.Instance;
    }

    public void SetTimeout(string commandType, TimeSpan timeout)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandType);

        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be positive.");

        _timeouts[commandType] = timeout;
    }

    public void SetDefaultTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be positive.");

        _timeouts["*"] = timeout;
    }

    public async Task<TimeoutResult<T>> ExecuteAsync<T>(
        string commandType,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        var timeout = ResolveTimeout(commandType);

        using var timeoutCts = new CancellationTokenSource(timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            timeoutCts.Token, cancellationToken);

        var started = _clock.UtcNowOffset;

        try
        {
            var result = await operation(linkedCts.Token);

            return new TimeoutResult<T>
            {
                Value = result,
                Success = true,
                TimedOut = false,
                Elapsed = _clock.UtcNowOffset - started
            };
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            return new TimeoutResult<T>
            {
                Success = false,
                TimedOut = true,
                Elapsed = _clock.UtcNowOffset - started,
                ErrorMessage = $"Operation timed out after {timeout.TotalSeconds:F1}s for command type '{commandType}'."
            };
        }
    }

    private TimeSpan ResolveTimeout(string commandType)
    {
        if (_timeouts.TryGetValue(commandType, out var timeout))
            return timeout;

        if (_timeouts.TryGetValue("*", out var defaultOverride))
            return defaultOverride;

        return _defaultTimeout;
    }
}

public sealed record TimeoutResult<T>
{
    public T? Value { get; init; }
    public required bool Success { get; init; }
    public required bool TimedOut { get; init; }
    public TimeSpan Elapsed { get; init; }
    public string? ErrorMessage { get; init; }
}
