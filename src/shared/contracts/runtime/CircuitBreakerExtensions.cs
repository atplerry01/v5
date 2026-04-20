namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// R2.A.D.3b / R-CIRCUIT-BREAKER-VOID-EXT-01 — ergonomic void-returning
/// helper for side-effect-only operations (e.g. <c>IProducer.ProduceAsync</c>
/// returning <see cref="Task"/>).
///
/// Forwards to the core <see cref="ICircuitBreaker.ExecuteAsync{T}"/> using
/// a trivial <see cref="int"/> return value. Extension-only — the
/// <see cref="ICircuitBreaker"/> interface is unchanged, so implementations
/// (current <c>DeterministicCircuitBreaker</c>, future fakes) are not
/// affected.
/// </summary>
public static class CircuitBreakerExtensions
{
    public static async Task ExecuteAsync(
        this ICircuitBreaker breaker,
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(breaker);
        ArgumentNullException.ThrowIfNull(operation);

        await breaker.ExecuteAsync<int>(async ct =>
        {
            await operation(ct);
            return 0;
        }, cancellationToken);
    }
}
