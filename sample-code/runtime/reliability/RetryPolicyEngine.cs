using System.Collections.Concurrent;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.Reliability;

public sealed record RetryPolicy
{
    public int MaxRetries { get; init; } = 3;
    public TimeSpan InitialDelay { get; init; } = TimeSpan.FromMilliseconds(200);
    public TimeSpan MaxDelay { get; init; } = TimeSpan.FromSeconds(10);
    public double BackoffMultiplier { get; init; } = 2.0;
    public Func<Exception, bool>? RetryableExceptionFilter { get; init; }
}

public sealed record RetryAttempt
{
    public required int AttemptNumber { get; init; }
    public required DateTimeOffset AttemptedAt { get; init; }
    public required bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public TimeSpan Elapsed { get; init; }
}

public sealed class RetryPolicyEngine
{
    private readonly RetryPolicy _defaultPolicy = new();
    private readonly ConcurrentDictionary<string, RetryPolicy> _policies = new();
    private readonly IClock _clock;

    public RetryPolicyEngine(IClock? clock = null)
    {
        _clock = clock ?? SystemClock.Instance;
    }

    public void SetPolicy(string commandType, RetryPolicy policy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandType);
        ArgumentNullException.ThrowIfNull(policy);
        _policies[commandType] = policy;
    }

    public void SetDefaultPolicy(RetryPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        _policies["*"] = policy;
    }

    public async Task<RetryResult<T>> ExecuteAsync<T>(
        string commandType,
        Func<Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        var policy = ResolvePolicy(commandType);
        var attempts = new List<RetryAttempt>();

        for (var attempt = 0; attempt <= policy.MaxRetries; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (attempt > 0)
            {
                var delay = ComputeDelay(attempt, policy);
                await Task.Delay(delay, cancellationToken);
            }

            var started = _clock.UtcNowOffset;

            try
            {
                var result = await operation();

                attempts.Add(new RetryAttempt
                {
                    AttemptNumber = attempt + 1,
                    AttemptedAt = started,
                    Success = true,
                    Elapsed = _clock.UtcNowOffset - started
                });

                return new RetryResult<T>
                {
                    Value = result,
                    Success = true,
                    Attempts = attempts
                };
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                attempts.Add(new RetryAttempt
                {
                    AttemptNumber = attempt + 1,
                    AttemptedAt = started,
                    Success = false,
                    ErrorMessage = ex.Message,
                    Elapsed = _clock.UtcNowOffset - started
                });

                var isRetryable = policy.RetryableExceptionFilter?.Invoke(ex) ?? true;

                if (!isRetryable || attempt == policy.MaxRetries)
                {
                    return new RetryResult<T>
                    {
                        Success = false,
                        Attempts = attempts,
                        FinalException = ex
                    };
                }
            }
        }

        // Unreachable, but satisfies compiler
        return new RetryResult<T> { Success = false, Attempts = attempts };
    }

    private RetryPolicy ResolvePolicy(string commandType)
    {
        if (_policies.TryGetValue(commandType, out var policy))
            return policy;

        if (_policies.TryGetValue("*", out var defaultOverride))
            return defaultOverride;

        return _defaultPolicy;
    }

    private static TimeSpan ComputeDelay(int attempt, RetryPolicy policy)
    {
        var delay = policy.InitialDelay.TotalMilliseconds
            * Math.Pow(policy.BackoffMultiplier, attempt - 1);

        return TimeSpan.FromMilliseconds(
            Math.Min(delay, policy.MaxDelay.TotalMilliseconds));
    }
}

public sealed record RetryResult<T>
{
    public T? Value { get; init; }
    public required bool Success { get; init; }
    public required List<RetryAttempt> Attempts { get; init; }
    public Exception? FinalException { get; init; }
    public int TotalAttempts => Attempts.Count;
}
