using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Tests.Integration.Setup;

/// <summary>
/// R3.A.6 incidental test-infrastructure helper: a pass-through
/// <see cref="ICircuitBreaker"/> that always executes the operation
/// and never opens. Used by integration-test harnesses that need to
/// satisfy the post-R2.A.D.3 constructor signatures on
/// <c>WhyceChainPostgresAdapter</c> and <c>RedisExecutionLockProvider</c>
/// without pulling in the deterministic-breaker wiring. Breaker
/// behaviour is covered by dedicated resilience tests, not by
/// harnesses that use this type.
/// </summary>
public sealed class NoOpTestCircuitBreaker : ICircuitBreaker
{
    public NoOpTestCircuitBreaker(string name) { Name = name; }

    public string Name { get; }

    public CircuitBreakerState State => CircuitBreakerState.Closed;

    public Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
        => operation(cancellationToken);
}
