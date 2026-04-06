using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Reliability;

public enum CircuitState
{
    Closed,
    Open,
    HalfOpen
}

public sealed record CircuitBreakerOptions
{
    public int FailureThreshold { get; init; } = 5;
    public TimeSpan OpenDuration { get; init; } = TimeSpan.FromSeconds(30);
    public int HalfOpenMaxAttempts { get; init; } = 1;
    public Func<Exception, bool>? ShouldTrip { get; init; }
}

/// <summary>
/// Circuit breaker for protecting downstream engine/service calls from cascade failures.
///
/// States:
///   Closed  — normal operation, failures are counted.
///   Open    — calls are rejected immediately for the configured duration.
///   HalfOpen — a limited number of trial calls are allowed to test recovery.
/// </summary>
public sealed class CircuitBreaker
{
    private readonly CircuitBreakerOptions _options;
    private readonly IClock _clock;
    private readonly object _lock = new();

    private CircuitState _state = CircuitState.Closed;
    private int _failureCount;
    private int _halfOpenAttempts;
    private DateTimeOffset _openedAt;

    public CircuitBreaker() : this(new CircuitBreakerOptions()) { }

    public CircuitBreaker(CircuitBreakerOptions options, IClock? clock = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
        _clock = clock ?? SystemClock.Instance;
    }

    public CircuitState State
    {
        get
        {
            lock (_lock)
            {
                if (_state == CircuitState.Open && _clock.UtcNowOffset - _openedAt >= _options.OpenDuration)
                {
                    _state = CircuitState.HalfOpen;
                    _halfOpenAttempts = 0;
                }
                return _state;
            }
        }
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var currentState = State;

        if (currentState == CircuitState.Open)
        {
            throw new CircuitBreakerOpenException(
                $"Circuit breaker is open. Retry after {_options.OpenDuration.TotalSeconds:F0}s.",
                _openedAt + _options.OpenDuration);
        }

        if (currentState == CircuitState.HalfOpen)
        {
            lock (_lock)
            {
                if (_halfOpenAttempts >= _options.HalfOpenMaxAttempts)
                {
                    throw new CircuitBreakerOpenException(
                        "Circuit breaker is half-open and max trial attempts reached.",
                        _openedAt + _options.OpenDuration);
                }
                _halfOpenAttempts++;
            }
        }

        try
        {
            var result = await operation();
            OnSuccess();
            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not CircuitBreakerOpenException)
        {
            OnFailure(ex);
            throw;
        }
    }

    public async Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(async () =>
        {
            await operation();
            return true;
        }, cancellationToken);
    }

    private void OnSuccess()
    {
        lock (_lock)
        {
            _failureCount = 0;
            _state = CircuitState.Closed;
        }
    }

    private void OnFailure(Exception ex)
    {
        var shouldTrip = _options.ShouldTrip?.Invoke(ex) ?? true;
        if (!shouldTrip) return;

        lock (_lock)
        {
            _failureCount++;

            if (_state == CircuitState.HalfOpen || _failureCount >= _options.FailureThreshold)
            {
                _state = CircuitState.Open;
                _openedAt = _clock.UtcNowOffset;
                _failureCount = 0;
            }
        }
    }
}

public sealed class CircuitBreakerOpenException : Exception
{
    public DateTimeOffset RetryAfter { get; }

    public CircuitBreakerOpenException(string message, DateTimeOffset retryAfter)
        : base(message)
    {
        RetryAfter = retryAfter;
    }
}
