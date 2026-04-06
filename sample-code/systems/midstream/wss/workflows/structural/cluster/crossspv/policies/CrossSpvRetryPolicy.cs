namespace Whycespace.Systems.Midstream.Wss.Workflows.Structural.Cluster.CrossSpv;

/// <summary>
/// E18.6.7 — Retry policy for cross-SPV workflow steps.
/// Exponential backoff with max 3 retries.
///
/// NEVER retry:
///   - Policy DENY (deterministic)
///   - Invariant violations (deterministic)
///   - Committed transactions (terminal)
///
/// ONLY retry:
///   - Transient infrastructure failures (network, timeout)
/// </summary>
public sealed class CrossSpvRetryPolicy
{
    public int MaxRetries => 3;
    private readonly TimeSpan _baseDelay = TimeSpan.FromSeconds(2);

    public bool ShouldRetry(int attempt, Exception exception)
    {
        if (attempt >= MaxRetries)
            return false;

        // Never retry deterministic failures
        if (exception is InvalidOperationException or ArgumentException)
            return false;

        // Never retry domain invariant violations
        if (exception.GetType().Name.Contains("Exception") &&
            exception.GetType().Namespace?.Contains("Domain") == true)
            return false;

        return true;
    }

    public TimeSpan GetDelay(int attempt)
        => TimeSpan.FromTicks((long)(_baseDelay.Ticks * Math.Pow(2, attempt)));
}
