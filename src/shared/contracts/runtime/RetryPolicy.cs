namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// Graduated retry policy for workflow steps that interact with external
/// or infrastructure-dependent systems. Provides backoff delays between
/// attempts and distinguishes between transient failures (retryable) and
/// permanent failures (escalate immediately).
///
/// Escalation model:
///   Attempt 1: immediate
///   Attempt 2: after <see cref="InitialDelayMs"/> ms
///   Attempt 3: after <see cref="InitialDelayMs"/> * 2 ms
///   All failed: return <see cref="RetryOutcome.Exhausted"/> for recovery queue escalation.
///
/// The policy does NOT compensate — that is the caller's responsibility
/// after receiving <see cref="RetryOutcome.Exhausted"/>. Callers may also
/// receive <see cref="RetryOutcome.PermanentFailure"/> for non-retryable
/// errors (detected via <see cref="IsPermanentFailure"/>).
/// </summary>
public sealed class RetryPolicy
{
    public int MaxAttempts { get; init; } = 3;
    public int InitialDelayMs { get; init; } = 200;

    /// <summary>
    /// Returns the delay (in ms) before the given attempt number (1-based).
    /// Attempt 1 has no delay; subsequent attempts use exponential backoff.
    /// </summary>
    public int GetDelayMs(int attempt) =>
        attempt <= 1 ? 0 : InitialDelayMs * (1 << (attempt - 2));

    /// <summary>
    /// Heuristic: detects permanently non-retryable errors from the error string.
    /// These errors will never succeed on retry regardless of delay.
    /// </summary>
    public static bool IsPermanentFailure(string? error) =>
        error is not null && (
            error.Contains("validation", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("already exists", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("WHYCEPOLICY", StringComparison.Ordinal));
}

public enum RetryOutcome
{
    Success,
    Exhausted,
    PermanentFailure,
    Cancelled
}
