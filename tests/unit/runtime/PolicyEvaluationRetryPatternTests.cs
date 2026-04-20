using Whycespace.Runtime.Resilience;
using Whycespace.Shared.Contracts.Infrastructure.Policy;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Tests.Unit.Runtime;

/// <summary>
/// R2.A.OPA / R-POL-OPA-RETRY-01 — pins the retry-around-evaluator pattern
/// that <c>PolicyMiddleware</c> uses to wrap <see cref="IPolicyEvaluator"/>
/// calls in <see cref="IRetryExecutor"/>. Three scenarios:
///
/// <list type="number">
///   <item>OPA throws once then succeeds → retry surfaces the eventual decision.</item>
///   <item>OPA throws persistently → retry exhausts and re-throws a fresh
///         <see cref="PolicyEvaluationUnavailableException"/> with
///         <c>RetryAfterSeconds</c> preserved.</item>
///   <item>OPA returns a genuine deny → retry is not engaged (decision passes through).</item>
/// </list>
///
/// These tests exercise the PATTERN (retry + classify + re-throw) using the
/// same <see cref="DeterministicRetryExecutor"/> that <c>PolicyMiddleware</c>
/// consumes. The architecture guard <c>PolicyMiddleware_Catches_PolicyEvaluationUnavailableException</c>
/// in <c>WbsmArchitectureTests</c> pins the actual middleware integration.
/// </summary>
public sealed class PolicyEvaluationRetryPatternTests
{
    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.Parse("2026-04-19T00:00:00Z");
    }

    // Stub evaluator: script the sequence of outcomes via a queue.
    private sealed class ScriptedPolicyEvaluator : IPolicyEvaluator
    {
        private readonly Queue<Func<PolicyDecision>> _outcomes = new();
        public int CallCount { get; private set; }

        public void Enqueue(Func<PolicyDecision> outcome) => _outcomes.Enqueue(outcome);

        public Task<PolicyDecision> EvaluateAsync(string policyId, object command, PolicyContext policyContext)
        {
            CallCount++;
            if (_outcomes.Count == 0)
                throw new InvalidOperationException("ScriptedPolicyEvaluator: no scripted outcome remaining.");
            var next = _outcomes.Dequeue();
            return Task.FromResult(next());
        }
    }

    private static PolicyContext NewContext() =>
        new(Guid.Parse("00000000-0000-0000-0000-000000000001"),
            "tenant",
            "actor",
            "FooCommand",
            new[] { "admin" },
            "operational",
            "sandbox",
            "todo");

    private static PolicyDecision Allow(string id = "p") =>
        new(true, id, "hash-allow", null);

    private static PolicyDecision Deny(string id = "p", string reason = "no") =>
        new(false, id, "hash-deny", reason);

    /// <summary>
    /// The exact retry wrapper used by <c>PolicyMiddleware.EvaluateOpaWithRetryAsync</c>.
    /// Factored into a helper so these tests exercise the canonical shape
    /// without instantiating the full middleware.
    /// </summary>
    private static async Task<PolicyDecision> EvaluateWithRetryPattern(
        IRetryExecutor executor,
        IPolicyEvaluator evaluator,
        string policyId,
        object command,
        PolicyContext policyContext,
        RetryPolicy? policy = null,
        CancellationToken cancellationToken = default)
    {
        PolicyEvaluationUnavailableException? lastUnavailable = null;

        var retryCtx = new RetryOperationContext
        {
            OperationId = $"{policyContext.CorrelationId:N}:opa:{policyId}",
            Policy = policy ?? new RetryPolicy { MaxAttempts = 3, InitialDelayMs = 10 },
            OperationName = "opa-evaluate"
        };

        var retryResult = await executor.ExecuteAsync<PolicyDecision>(
            retryCtx,
            async (attempt, ct) =>
            {
                try
                {
                    var decision = await evaluator.EvaluateAsync(policyId, command, policyContext);
                    return RetryStepResult<PolicyDecision>.Success(decision);
                }
                catch (PolicyEvaluationUnavailableException ex)
                {
                    lastUnavailable = ex;
                    return RetryStepResult<PolicyDecision>.Failure(
                        RuntimeFailureCategory.PolicyEvaluationDeferred,
                        $"{ex.Reason}: {ex.Message}");
                }
            },
            cancellationToken);

        if (retryResult.Outcome == RetryOutcome.Success)
            return retryResult.Value!;

        if (lastUnavailable is not null)
        {
            throw new PolicyEvaluationUnavailableException(
                reason: $"retry_exhausted:{lastUnavailable.Reason}",
                retryAfterSeconds: lastUnavailable.RetryAfterSeconds,
                message: $"OPA policy evaluation for '{policyId}' failed after {retryResult.AttemptsMade} attempts. Last reason: {lastUnavailable.Reason}. {lastUnavailable.Message}",
                innerException: lastUnavailable);
        }

        throw new InvalidOperationException(
            $"Policy evaluation retry exhausted without captured unavailable exception.");
    }

    private static DeterministicRetryExecutor NewExecutor() =>
        new(new FakeClock(), new DeterministicRandomProvider());

    // ─────────────────────────────────────────────────────────────────────
    // Scenario 1 — transient failure → retry surfaces eventual decision.
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Retries_On_OPA_Unavailable_And_Returns_Decision_On_Second_Attempt()
    {
        var executor = NewExecutor();
        var evaluator = new ScriptedPolicyEvaluator();

        // Attempt 1: throw (timeout). Attempt 2: allow.
        evaluator.Enqueue(() => throw new PolicyEvaluationUnavailableException(
            reason: "timeout", retryAfterSeconds: 5, message: "OPA timeout"));
        evaluator.Enqueue(() => Allow("p1"));

        var decision = await EvaluateWithRetryPattern(
            executor, evaluator, "p1", new object(), NewContext());

        Assert.True(decision.IsAllowed);
        Assert.Equal("p1", decision.PolicyId);
        Assert.Equal(2, evaluator.CallCount);
    }

    [Fact]
    public async Task Retries_Surface_Deny_Decision_When_Recovered()
    {
        var executor = NewExecutor();
        var evaluator = new ScriptedPolicyEvaluator();

        // Throw twice, then the evaluator genuinely denies. Retry path
        // returns the deny decision — NOT a silent allow. Policy primacy ($8).
        evaluator.Enqueue(() => throw new PolicyEvaluationUnavailableException(
            reason: "transport", retryAfterSeconds: 5, message: "network blip"));
        evaluator.Enqueue(() => throw new PolicyEvaluationUnavailableException(
            reason: "transport", retryAfterSeconds: 5, message: "still blipping"));
        evaluator.Enqueue(() => Deny("p1", "insufficient-role"));

        var decision = await EvaluateWithRetryPattern(
            executor, evaluator, "p1", new object(), NewContext());

        Assert.False(decision.IsAllowed);
        Assert.Equal("insufficient-role", decision.DenialReason);
        Assert.Equal(3, evaluator.CallCount);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Scenario 2 — exhaustion re-throws with preserved RetryAfterSeconds.
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Exhausts_Retry_And_Rethrows_PolicyEvaluationUnavailable_With_Retry_Hint_Preserved()
    {
        var executor = NewExecutor();
        var evaluator = new ScriptedPolicyEvaluator();

        // All 3 attempts throw. The final exception carries RetryAfter=42s;
        // the re-thrown exception MUST preserve that value for the 503 header.
        for (int i = 0; i < 3; i++)
        {
            evaluator.Enqueue(() => throw new PolicyEvaluationUnavailableException(
                reason: "breaker_open", retryAfterSeconds: 42, message: "OPA breaker open"));
        }

        var ex = await Assert.ThrowsAsync<PolicyEvaluationUnavailableException>(() =>
            EvaluateWithRetryPattern(executor, evaluator, "p1", new object(), NewContext()));

        Assert.StartsWith("retry_exhausted:", ex.Reason);
        Assert.Contains("breaker_open", ex.Reason);
        Assert.Equal(42, ex.RetryAfterSeconds);
        Assert.Contains("3 attempts", ex.Message);
        Assert.Equal(3, evaluator.CallCount);
        // Inner exception preserved for diagnostics.
        Assert.IsType<PolicyEvaluationUnavailableException>(ex.InnerException);
    }

    [Fact]
    public async Task Exhaustion_Final_Exception_Preserves_Last_RetryAfter_Not_First()
    {
        var executor = NewExecutor();
        var evaluator = new ScriptedPolicyEvaluator();

        evaluator.Enqueue(() => throw new PolicyEvaluationUnavailableException(
            reason: "timeout", retryAfterSeconds: 5, message: "first"));
        evaluator.Enqueue(() => throw new PolicyEvaluationUnavailableException(
            reason: "transport", retryAfterSeconds: 10, message: "second"));
        evaluator.Enqueue(() => throw new PolicyEvaluationUnavailableException(
            reason: "breaker_open", retryAfterSeconds: 30, message: "third"));

        var ex = await Assert.ThrowsAsync<PolicyEvaluationUnavailableException>(() =>
            EvaluateWithRetryPattern(executor, evaluator, "p1", new object(), NewContext()));

        // The LAST exception's reason + retry-after wins — that's the most
        // recent signal about when to try again.
        Assert.Contains("breaker_open", ex.Reason);
        Assert.Equal(30, ex.RetryAfterSeconds);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Scenario 3 — genuine deny does NOT trigger retry.
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Does_Not_Retry_On_Genuine_OPA_Deny()
    {
        var executor = NewExecutor();
        var evaluator = new ScriptedPolicyEvaluator();

        // Evaluator cleanly denies on attempt 1 — no exception thrown.
        // Retry executor sees Success outcome and returns immediately.
        evaluator.Enqueue(() => Deny("p1", "no-permission"));

        var decision = await EvaluateWithRetryPattern(
            executor, evaluator, "p1", new object(), NewContext());

        Assert.False(decision.IsAllowed);
        Assert.Equal("no-permission", decision.DenialReason);
        Assert.Equal(1, evaluator.CallCount); // critical: no retry on deny
    }

    [Fact]
    public async Task Does_Not_Retry_On_Genuine_Allow()
    {
        var executor = NewExecutor();
        var evaluator = new ScriptedPolicyEvaluator();

        evaluator.Enqueue(() => Allow("p1"));

        var decision = await EvaluateWithRetryPattern(
            executor, evaluator, "p1", new object(), NewContext());

        Assert.True(decision.IsAllowed);
        Assert.Equal(1, evaluator.CallCount);
    }
}
