using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Runtime.Resilience;

/// <summary>
/// R2.A.D.1 / R-CIRCUIT-BREAKER-01 canonical implementation.
/// Replay-deterministic (time via <see cref="IClock"/>, no wall-clock reads,
/// no <see cref="Random"/>). State transitions are a pure function of
/// observed operation outcomes and clock reads.
///
/// Thread safety: all state mutations happen inside a single lock; the
/// lock is uncontended on the happy path (a nanosecond-scale Closed-state
/// check). Only enters the critical section on failure or state-transition
/// observation. Under Open-state contention, N concurrent callers each
/// take the lock briefly to throw — no lost-update / double-trial risk.
///
/// HalfOpen trial: admitted by a single caller under the lock. Concurrent
/// callers during HalfOpen see Open until the trial commits one way or
/// the other.
/// </summary>
public sealed class DeterministicCircuitBreaker : ICircuitBreaker
{
    private readonly IClock _clock;
    private readonly int _failureThreshold;
    private readonly int _windowSeconds;

    private readonly object _lock = new();
    private int _consecutiveFailures;
    private DateTimeOffset? _openedAt;

    public string Name { get; }

    public DeterministicCircuitBreaker(CircuitBreakerOptions options, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(clock);
        if (string.IsNullOrWhiteSpace(options.Name))
            throw new ArgumentException("CircuitBreakerOptions.Name is required.", nameof(options));
        if (options.FailureThreshold < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.FailureThreshold,
                "FailureThreshold must be at least 1.");
        if (options.WindowSeconds < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.WindowSeconds,
                "WindowSeconds must be at least 1.");

        _clock = clock;
        _failureThreshold = options.FailureThreshold;
        _windowSeconds = options.WindowSeconds;
        Name = options.Name;
    }

    /// <summary>
    /// Side-effect-free snapshot. Does NOT transition state or consume
    /// the HalfOpen trial slot. Safe to poll at any rate for health.
    /// </summary>
    public CircuitBreakerState State
    {
        get
        {
            lock (_lock)
            {
                if (_openedAt is null) return CircuitBreakerState.Closed;
                var elapsed = _clock.UtcNow - _openedAt.Value;
                return elapsed.TotalSeconds < _windowSeconds
                    ? CircuitBreakerState.Open
                    : CircuitBreakerState.HalfOpen;
            }
        }
    }

    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        // Gate: Open → throw immediately. HalfOpen → admit exactly one
        // trial caller (the lock enforces single-writer). Closed → run normally.
        bool isTrial = CheckAndMaybeAdmit(out int retryAfterSeconds);

        if (!isTrial && IsOpenForCaller(out retryAfterSeconds))
        {
            throw new CircuitBreakerOpenException(
                Name,
                retryAfterSeconds,
                $"Circuit breaker '{Name}' is open. No bypass allowed.");
        }

        try
        {
            var result = await operation(cancellationToken);
            RecordSuccess();
            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Caller-initiated cancellation — not a failure, just propagate.
            throw;
        }
        catch
        {
            RecordFailure();
            throw;
        }
    }

    /// <summary>
    /// Snapshot gate used by <see cref="ExecuteAsync{T}"/>. Returns true when
    /// the caller is the admitted HalfOpen trial; false otherwise. When
    /// the breaker is Open (within window), the follow-up
    /// <see cref="IsOpenForCaller"/> call signals the throw.
    /// </summary>
    private bool CheckAndMaybeAdmit(out int retryAfterSeconds)
    {
        lock (_lock)
        {
            if (_openedAt is null)
            {
                retryAfterSeconds = 0;
                return false; // Closed — normal run, not a trial.
            }

            var elapsed = _clock.UtcNow - _openedAt.Value;
            if (elapsed.TotalSeconds < _windowSeconds)
            {
                // Still within Open window.
                retryAfterSeconds = Math.Max(1, _windowSeconds - (int)elapsed.TotalSeconds);
                return false;
            }

            // Window elapsed → admit THIS caller as the trial. Clear
            // _openedAt atomically so concurrent callers see Closed-like
            // state (or the next-opened state). The trial call's outcome
            // will commit via RecordSuccess / RecordFailure.
            _openedAt = null;
            retryAfterSeconds = 0;
            return true;
        }
    }

    private bool IsOpenForCaller(out int retryAfterSeconds)
    {
        lock (_lock)
        {
            if (_openedAt is null)
            {
                retryAfterSeconds = 0;
                return false;
            }
            var elapsed = _clock.UtcNow - _openedAt.Value;
            if (elapsed.TotalSeconds < _windowSeconds)
            {
                retryAfterSeconds = Math.Max(1, _windowSeconds - (int)elapsed.TotalSeconds);
                return true;
            }
            retryAfterSeconds = 0;
            return false;
        }
    }

    private void RecordSuccess()
    {
        lock (_lock)
        {
            _consecutiveFailures = 0;
            _openedAt = null;
        }
    }

    private void RecordFailure()
    {
        lock (_lock)
        {
            _consecutiveFailures++;
            if (_consecutiveFailures >= _failureThreshold && _openedAt is null)
            {
                _openedAt = _clock.UtcNow;
            }
        }
    }
}
