using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Runtime.Resilience;

/// <summary>
/// R2.A.1 canonical <see cref="IRetryExecutor"/> implementation.
/// Replay-deterministic: time via <see cref="IClock"/>, jitter via
/// <see cref="IRandomProvider"/> seeded from the operation id + attempt.
///
/// Algorithm per attempt (1-based):
///   delay_0 = 0 for attempt 1
///   delay_n = InitialDelayMs * 2^(n-2)              (exponential base)
///           + jitter(n) where jitter(n) =
///             IRandomProvider.NextDouble($"{opId}:retry-jitter:{n}")
///               * base_delay_n * JitterFraction
///
/// The executor wraps the inner operation in try/catch; any exception is
/// mapped to <see cref="RuntimeFailureCategory.ExecutionFailure"/> so the
/// canonical category table drives retry eligibility. Callers that want
/// typed mapping should return <see cref="RetryStepResult{T}.Failure"/>
/// themselves (e.g. via <c>RuntimeExceptionMapper.Map</c>) before the
/// executor sees the outcome.
/// </summary>
public sealed class DeterministicRetryExecutor : IRetryExecutor
{
    /// <summary>
    /// Fraction of base delay used as jitter envelope. ±20% of the base,
    /// deterministic per seed. Matches typical AWS SDK / resilience-library
    /// defaults while staying replay-safe.
    /// </summary>
    public const double JitterFraction = 0.2;

    private readonly IClock _clock;
    private readonly IRandomProvider _random;

    public DeterministicRetryExecutor(IClock clock, IRandomProvider random)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(random);
        _clock = clock;
        _random = random;
    }

    public async Task<RetryResult<T>> ExecuteAsync<T>(
        RetryOperationContext context,
        Func<int, CancellationToken, Task<RetryStepResult<T>>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(operation);
        if (string.IsNullOrWhiteSpace(context.OperationId))
            throw new ArgumentException("OperationId is required.", nameof(context));

        var attempts = new List<RetryAttemptRecord>(context.Policy.MaxAttempts);

        for (int attempt = 1; attempt <= context.Policy.MaxAttempts; attempt++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                var cancelAt = _clock.UtcNow;
                attempts.Add(new RetryAttemptRecord(
                    AttemptNumber: attempt,
                    StartedAt: cancelAt,
                    CompletedAt: cancelAt,
                    DelayBeforeAttempt: TimeSpan.Zero,
                    IsSuccess: false,
                    FailureCategory: RuntimeFailureCategory.Cancellation,
                    Error: "Operation cancelled before attempt."));

                return new RetryResult<T>
                {
                    Outcome = RetryOutcome.Cancelled,
                    AttemptsMade = attempt - 1,
                    FinalFailureCategory = RuntimeFailureCategory.Cancellation,
                    FinalError = "Operation cancelled.",
                    Attempts = attempts
                };
            }

            var delay = ComputeDelay(context, attempt);
            if (delay > TimeSpan.Zero)
            {
                try
                {
                    await Task.Delay(delay, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    var cancelAt = _clock.UtcNow;
                    attempts.Add(new RetryAttemptRecord(
                        AttemptNumber: attempt,
                        StartedAt: cancelAt,
                        CompletedAt: cancelAt,
                        DelayBeforeAttempt: delay,
                        IsSuccess: false,
                        FailureCategory: RuntimeFailureCategory.Cancellation,
                        Error: "Operation cancelled during backoff."));

                    return new RetryResult<T>
                    {
                        Outcome = RetryOutcome.Cancelled,
                        AttemptsMade = attempt - 1,
                        FinalFailureCategory = RuntimeFailureCategory.Cancellation,
                        FinalError = "Operation cancelled.",
                        Attempts = attempts
                    };
                }
            }

            var startedAt = _clock.UtcNow;
            RetryStepResult<T> step;
            try
            {
                step = await operation(attempt, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                var cancelAt = _clock.UtcNow;
                attempts.Add(new RetryAttemptRecord(
                    AttemptNumber: attempt,
                    StartedAt: startedAt,
                    CompletedAt: cancelAt,
                    DelayBeforeAttempt: delay,
                    IsSuccess: false,
                    FailureCategory: RuntimeFailureCategory.Cancellation,
                    Error: "Operation cancelled during attempt."));

                return new RetryResult<T>
                {
                    Outcome = RetryOutcome.Cancelled,
                    AttemptsMade = attempt,
                    FinalFailureCategory = RuntimeFailureCategory.Cancellation,
                    FinalError = "Operation cancelled.",
                    Attempts = attempts
                };
            }
            catch (Exception ex)
            {
                step = RetryStepResult<T>.Failure(
                    RuntimeFailureCategory.ExecutionFailure,
                    ex.Message);
            }

            var completedAt = _clock.UtcNow;
            attempts.Add(new RetryAttemptRecord(
                AttemptNumber: attempt,
                StartedAt: startedAt,
                CompletedAt: completedAt,
                DelayBeforeAttempt: delay,
                IsSuccess: step.IsSuccess,
                FailureCategory: step.FailureCategory,
                Error: step.Error));

            if (step.IsSuccess)
            {
                return new RetryResult<T>
                {
                    Outcome = RetryOutcome.Success,
                    Value = step.Value,
                    AttemptsMade = attempt,
                    Attempts = attempts
                };
            }

            // Terminal classifications short-circuit — further attempts would not change the outcome.
            if (!RetryEligibility.IsRetryable(step.FailureCategory))
            {
                return new RetryResult<T>
                {
                    Outcome = RetryOutcome.PermanentFailure,
                    AttemptsMade = attempt,
                    FinalFailureCategory = step.FailureCategory,
                    FinalError = step.Error,
                    Attempts = attempts
                };
            }
        }

        var lastAttempt = attempts[^1];
        return new RetryResult<T>
        {
            Outcome = RetryOutcome.Exhausted,
            AttemptsMade = context.Policy.MaxAttempts,
            FinalFailureCategory = lastAttempt.FailureCategory,
            FinalError = lastAttempt.Error,
            Attempts = attempts
        };
    }

    private TimeSpan ComputeDelay(RetryOperationContext context, int attempt)
    {
        if (attempt <= 1) return TimeSpan.Zero;

        var baseMs = context.Policy.GetDelayMs(attempt);
        if (baseMs <= 0) return TimeSpan.Zero;

        // Deterministic jitter: seed derived from operation id + attempt.
        // Same seed → same jitter on replay. Seed NEVER includes clock or
        // Guid.NewGuid.
        var seed = $"{context.OperationId}:retry-jitter:{attempt}";
        var jitterUnit = _random.NextDouble(seed);        // [0.0, 1.0)
        var jitterMs = baseMs * JitterFraction * jitterUnit;

        return TimeSpan.FromMilliseconds(baseMs + jitterMs);
    }
}
