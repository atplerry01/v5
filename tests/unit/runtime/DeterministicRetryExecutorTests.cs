using Whycespace.Runtime.Resilience;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Tests.Unit.Runtime;

/// <summary>
/// R2.A.1 — pin the four retry-primitive rules (R-RETRY-DET-01,
/// R-RETRY-CAT-01, R-RETRY-CAP-01, R-RETRY-EVIDENCE-01) against the
/// <see cref="DeterministicRetryExecutor"/> implementation.
///
/// These tests are the canonical proof that:
///   1. Retry behaviour is replay-deterministic (same context + same
///      inner outcomes → same result).
///   2. Retry eligibility is category-driven, never string-based.
///   3. The executor is bounded — no infinite loops.
///   4. Every attempt produces auditable evidence.
/// </summary>
public sealed class DeterministicRetryExecutorTests
{
    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.Parse("2026-04-19T00:00:00Z");

        public void Advance(TimeSpan by) => UtcNow = UtcNow.Add(by);
    }

    private static RetryOperationContext NewContext(
        string opId = "op-test",
        int maxAttempts = 3,
        int initialDelayMs = 200)
    {
        return new RetryOperationContext
        {
            OperationId = opId,
            Policy = new RetryPolicy { MaxAttempts = maxAttempts, InitialDelayMs = initialDelayMs },
            OperationName = "test-operation"
        };
    }

    private static DeterministicRetryExecutor NewExecutor(FakeClock? clock = null)
    {
        clock ??= new FakeClock();
        var random = new DeterministicRandomProvider();
        return new DeterministicRetryExecutor(clock, random);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Success path
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Success_On_First_Attempt_Returns_Value_With_One_Record()
    {
        var executor = NewExecutor();
        var ctx = NewContext();

        var result = await executor.ExecuteAsync<int>(
            ctx,
            (attempt, _) => Task.FromResult(RetryStepResult<int>.Success(42)));

        Assert.Equal(RetryOutcome.Success, result.Outcome);
        Assert.Equal(42, result.Value);
        Assert.Equal(1, result.AttemptsMade);
        Assert.Single(result.Attempts);
        Assert.True(result.Attempts[0].IsSuccess);
        Assert.Equal(TimeSpan.Zero, result.Attempts[0].DelayBeforeAttempt);
    }

    [Fact]
    public async Task Success_After_Retryable_Failures_Returns_Value_With_Full_Evidence()
    {
        var executor = NewExecutor();
        var ctx = NewContext();
        int calls = 0;

        var result = await executor.ExecuteAsync<int>(
            ctx,
            (attempt, _) =>
            {
                calls++;
                if (attempt < 3)
                    return Task.FromResult(RetryStepResult<int>.Failure(
                        RuntimeFailureCategory.DependencyUnavailable, "transient"));
                return Task.FromResult(RetryStepResult<int>.Success(7));
            });

        Assert.Equal(RetryOutcome.Success, result.Outcome);
        Assert.Equal(7, result.Value);
        Assert.Equal(3, result.AttemptsMade);
        Assert.Equal(3, calls);
        Assert.Equal(3, result.Attempts.Count);
        Assert.False(result.Attempts[0].IsSuccess);
        Assert.False(result.Attempts[1].IsSuccess);
        Assert.True(result.Attempts[2].IsSuccess);
    }

    // ─────────────────────────────────────────────────────────────────────
    // R-RETRY-CAT-01 — category-driven eligibility
    // ─────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(RuntimeFailureCategory.Timeout, true)]
    [InlineData(RuntimeFailureCategory.DependencyUnavailable, true)]
    [InlineData(RuntimeFailureCategory.ConcurrencyConflict, true)]
    [InlineData(RuntimeFailureCategory.ResourceExhausted, true)]
    [InlineData(RuntimeFailureCategory.ExecutionFailure, true)]
    [InlineData(RuntimeFailureCategory.PersistenceFailure, true)]
    [InlineData(RuntimeFailureCategory.AuthorizationDenied, false)]
    [InlineData(RuntimeFailureCategory.PolicyDenied, false)]
    [InlineData(RuntimeFailureCategory.ValidationFailed, false)]
    [InlineData(RuntimeFailureCategory.RuntimeGuardRejection, false)]
    [InlineData(RuntimeFailureCategory.InvalidState, false)]
    [InlineData(RuntimeFailureCategory.PoisonMessage, false)]
    [InlineData(RuntimeFailureCategory.Cancellation, false)]
    [InlineData(RuntimeFailureCategory.Unknown, false)]
    public void RetryEligibility_Canonical_Mapping(RuntimeFailureCategory category, bool expected)
    {
        Assert.Equal(expected, RetryEligibility.IsRetryable(category));
    }

    [Fact]
    public async Task Non_Retryable_Category_Short_Circuits_After_First_Attempt()
    {
        var executor = NewExecutor();
        var ctx = NewContext();
        int calls = 0;

        var result = await executor.ExecuteAsync<int>(
            ctx,
            (attempt, _) =>
            {
                calls++;
                return Task.FromResult(RetryStepResult<int>.Failure(
                    RuntimeFailureCategory.PolicyDenied, "policy deny"));
            });

        Assert.Equal(RetryOutcome.PermanentFailure, result.Outcome);
        Assert.Equal(1, result.AttemptsMade);
        Assert.Equal(1, calls);
        Assert.Equal(RuntimeFailureCategory.PolicyDenied, result.FinalFailureCategory);
        Assert.Equal("policy deny", result.FinalError);
    }

    // ─────────────────────────────────────────────────────────────────────
    // R-RETRY-CAP-01 — bounded attempts
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Sustained_Retryable_Failures_Exhaust_After_MaxAttempts()
    {
        var executor = NewExecutor();
        var ctx = NewContext(maxAttempts: 3);
        int calls = 0;

        var result = await executor.ExecuteAsync<int>(
            ctx,
            (attempt, _) =>
            {
                calls++;
                return Task.FromResult(RetryStepResult<int>.Failure(
                    RuntimeFailureCategory.DependencyUnavailable, "still down"));
            });

        Assert.Equal(RetryOutcome.Exhausted, result.Outcome);
        Assert.Equal(3, result.AttemptsMade);
        Assert.Equal(3, calls);
        Assert.Equal(3, result.Attempts.Count);
        Assert.Equal(RuntimeFailureCategory.DependencyUnavailable, result.FinalFailureCategory);
    }

    [Fact]
    public async Task MaxAttempts_Of_One_Runs_Once_And_Then_Exhausts()
    {
        var executor = NewExecutor();
        var ctx = NewContext(maxAttempts: 1);
        int calls = 0;

        var result = await executor.ExecuteAsync<int>(
            ctx,
            (_, _) =>
            {
                calls++;
                return Task.FromResult(RetryStepResult<int>.Failure(
                    RuntimeFailureCategory.Timeout, "timeout"));
            });

        Assert.Equal(RetryOutcome.Exhausted, result.Outcome);
        Assert.Equal(1, calls);
    }

    // ─────────────────────────────────────────────────────────────────────
    // R-RETRY-DET-01 — replay determinism
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Two_Runs_With_Same_Context_And_Outcomes_Produce_Identical_Delays()
    {
        // Rerun the same retry sequence twice; every delay (including jitter)
        // must be bit-identical since IRandomProvider is seed-driven.
        var executor1 = NewExecutor();
        var executor2 = NewExecutor();
        var ctx = NewContext(opId: "deterministic-test-op");

        Task<RetryStepResult<int>> InnerOp(int attempt, CancellationToken _) =>
            attempt < 3
                ? Task.FromResult(RetryStepResult<int>.Failure(
                    RuntimeFailureCategory.DependencyUnavailable, "down"))
                : Task.FromResult(RetryStepResult<int>.Success(42));

        var result1 = await executor1.ExecuteAsync<int>(ctx, InnerOp);
        var result2 = await executor2.ExecuteAsync<int>(ctx, InnerOp);

        Assert.Equal(result1.Outcome, result2.Outcome);
        Assert.Equal(result1.AttemptsMade, result2.AttemptsMade);
        Assert.Equal(result1.Attempts.Count, result2.Attempts.Count);

        for (int i = 0; i < result1.Attempts.Count; i++)
        {
            Assert.Equal(
                result1.Attempts[i].DelayBeforeAttempt,
                result2.Attempts[i].DelayBeforeAttempt);
        }
    }

    [Fact]
    public async Task Delay_Is_Zero_On_First_Attempt_And_Grows_Exponentially()
    {
        var executor = NewExecutor();
        var ctx = NewContext(maxAttempts: 3, initialDelayMs: 100);

        var result = await executor.ExecuteAsync<int>(
            ctx,
            (_, _) => Task.FromResult(RetryStepResult<int>.Failure(
                RuntimeFailureCategory.DependencyUnavailable, "down")));

        // Attempt 1: no delay.
        Assert.Equal(TimeSpan.Zero, result.Attempts[0].DelayBeforeAttempt);

        // Attempt 2: base 100ms + jitter up to 20% (120ms cap).
        var d2 = result.Attempts[1].DelayBeforeAttempt.TotalMilliseconds;
        Assert.InRange(d2, 100, 120);

        // Attempt 3: base 200ms + jitter up to 20% (240ms cap).
        var d3 = result.Attempts[2].DelayBeforeAttempt.TotalMilliseconds;
        Assert.InRange(d3, 200, 240);
    }

    // ─────────────────────────────────────────────────────────────────────
    // R-RETRY-EVIDENCE-01 — attempt records
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Attempt_Records_Contain_Every_Attempt_With_Timestamps()
    {
        var clock = new FakeClock();
        var executor = NewExecutor(clock);
        var ctx = NewContext(maxAttempts: 2, initialDelayMs: 10);

        var result = await executor.ExecuteAsync<int>(
            ctx,
            (attempt, _) =>
            {
                clock.Advance(TimeSpan.FromMilliseconds(5));
                return Task.FromResult(RetryStepResult<int>.Failure(
                    RuntimeFailureCategory.Timeout, $"timeout-{attempt}"));
            });

        Assert.Equal(2, result.Attempts.Count);

        // Every record has non-default timestamps and preserves the failure category.
        foreach (var record in result.Attempts)
        {
            Assert.NotEqual(default, record.StartedAt);
            Assert.NotEqual(default, record.CompletedAt);
            Assert.True(record.CompletedAt >= record.StartedAt);
            Assert.Equal(RuntimeFailureCategory.Timeout, record.FailureCategory);
            Assert.False(record.IsSuccess);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Exception -> ExecutionFailure classification
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Unclassified_Exception_Is_Treated_As_ExecutionFailure_And_Retried()
    {
        var executor = NewExecutor();
        var ctx = NewContext(maxAttempts: 2, initialDelayMs: 10);
        int calls = 0;

        var result = await executor.ExecuteAsync<int>(
            ctx,
            (attempt, _) =>
            {
                calls++;
                if (attempt == 1)
                    throw new InvalidOperationException("surprise");
                return Task.FromResult(RetryStepResult<int>.Success(99));
            });

        Assert.Equal(RetryOutcome.Success, result.Outcome);
        Assert.Equal(99, result.Value);
        Assert.Equal(2, calls);
        Assert.Equal(RuntimeFailureCategory.ExecutionFailure, result.Attempts[0].FailureCategory);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Cancellation propagation
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Cancellation_Before_Attempt_Returns_Cancelled_Outcome()
    {
        var executor = NewExecutor();
        var ctx = NewContext();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await executor.ExecuteAsync<int>(
            ctx,
            (_, _) => Task.FromResult(RetryStepResult<int>.Success(1)),
            cts.Token);

        Assert.Equal(RetryOutcome.Cancelled, result.Outcome);
    }
}
