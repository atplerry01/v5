using Whyce.Shared.Contracts.Application.Todo;
using Whyce.Shared.Contracts.Infrastructure.Policy;
using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.FailureRecovery;

/// <summary>
/// phase1.5-S5.2.6 / FR-4 (POLICY-ENGINE-FAILURE-01): proves that an
/// <see cref="IPolicyEvaluator"/> failure (OPA timeout, transport error,
/// breaker-open) is FAIL-CLOSED end-to-end:
///
///   - <see cref="PolicyEvaluationUnavailableException"/> bubbles
///     untouched through the runtime control plane (R-RT-04 typed-edge
///     refusal pattern).
///   - The aggregate event store has ZERO events for the test
///     aggregate (Step 7 of `RuntimeControlPlane.ExecuteAsync` —
///     `EventFabric.ProcessAsync` — never runs).
///   - The chain anchor produces ZERO blocks for the correlation id.
///   - The outbox produces ZERO batches for the correlation id.
///
/// In other words: a policy engine outage refuses the request at the
/// door and produces NO observable side effect anywhere downstream of
/// the policy middleware. This is the canonical $8 / GE-02 invariant
/// that "no engine receives an unauthorized command" — extended to
/// "no engine receives an UNDECIDED command either".
///
/// WHY THIS USES TestHost (NOT REAL POSTGRES):
///
///   The contract under test is the in-process bubble path:
///   PolicyMiddleware MUST throw, the runtime MUST NOT catch, the
///   downstream stages MUST NOT execute. Real Postgres adds nothing
///   to that proof — the assertion is "ZERO downstream side effects",
///   which the in-memory adapters answer with byte-perfect fidelity.
///   Using TestHost also avoids polluting the shared `outbox` table
///   with FR-4 test rows, so this class does NOT need
///   <see cref="OutboxSharedTableCollection"/>.
///
///   Determinism: TestHost defaults to TestClock + TestIdGenerator
///   (T-DOUBLES-01 compliant). The throwing stub takes no
///   non-deterministic primitives.
/// </summary>
public sealed class PolicyEngineFailureTest
{
    [Fact]
    public async Task Policy_Evaluator_Throwing_Is_Fail_Closed_With_No_Downstream_Side_Effects()
    {
        // ── Build a TestHost with a stub IPolicyEvaluator that throws
        // PolicyEvaluationUnavailableException("breaker_open", ...) on
        // every call. This is the exact exception type the production
        // OpaPolicyEvaluator throws on breaker-open / timeout / transport
        // failure (per src/platform/host/adapters/OpaPolicyEvaluator.cs). ──
        var throwingEvaluator = new ThrowingPolicyEvaluator();
        var host = TestHost.ForTodo(policyEvaluator: throwingEvaluator);

        var aggregateId = host.IdGenerator.Generate("fr4:agg");
        var context = host.NewTodoContext(aggregateId);
        var command = new CreateTodoCommand(aggregateId, "fr4-policy-failure-probe");

        // ── The exception MUST bubble untouched through the control
        // plane. R-RT-04's "Note" explicitly preserves typed-exception
        // bubble for PolicyEvaluationUnavailableException as an
        // edge-handler pattern. We assert exactly that here. ──
        var caught = await Assert.ThrowsAsync<PolicyEvaluationUnavailableException>(
            async () => await host.ControlPlane.ExecuteAsync(command, context));

        Assert.Equal("breaker_open", caught.Reason);
        Assert.Equal(1, throwingEvaluator.CallCount);

        // ── ZERO downstream side effects. The contract is that policy
        // failure refuses the request before persist / chain / outbox
        // run. Each in-memory adapter is observed via its public test
        // accessor and must report nothing for this aggregate / corrId. ──
        Assert.Empty(host.EventStore.AllEvents(aggregateId));

        // Chain anchor: the InMemoryChainAnchor is wired into the host
        // (the FR-4 override pertains to the policy evaluator only),
        // so we can assert directly that no block was produced for our
        // correlation id.
        Assert.DoesNotContain(host.ChainAnchor.Blocks,
            b => b.CorrelationId == context.CorrelationId);

        // Outbox: same — no batches enqueued for this correlation id.
        Assert.DoesNotContain(host.Outbox.Batches,
            b => b.CorrelationId == context.CorrelationId);
    }

    /// <summary>
    /// Stub <see cref="IPolicyEvaluator"/> that throws the canonical
    /// typed unavailability exception on every call. Mirrors the
    /// production breaker-open path so the test exercises the SAME
    /// exception type the production code emits — not a generic
    /// <see cref="Exception"/> that the middleware might handle
    /// differently.
    ///
    /// Counts calls so the test can assert the policy stage actually
    /// ran (vs. being skipped by an upstream short-circuit).
    /// </summary>
    private sealed class ThrowingPolicyEvaluator : IPolicyEvaluator
    {
        public int CallCount { get; private set; }

        public Task<PolicyDecision> EvaluateAsync(string policyId, object command, PolicyContext policyContext)
        {
            CallCount++;
            throw new PolicyEvaluationUnavailableException(
                reason: "breaker_open",
                retryAfterSeconds: 10,
                message: "FR-4 stub: simulated OPA breaker-open");
        }
    }
}
