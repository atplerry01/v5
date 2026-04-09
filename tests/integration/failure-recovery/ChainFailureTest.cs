using Whyce.Shared.Contracts.Application.Todo;
using Whyce.Shared.Contracts.Infrastructure.Chain;
using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.FailureRecovery;

/// <summary>
/// phase1.5-S5.2.6 / FR-5 (CHAIN-ANCHOR-FAILURE-01): proves that a
/// WhyceChain anchor failure obeys the documented invariants:
///
///   * Step 2 (EventStoreService.AppendAsync) ALREADY persisted the
///     events to the source of truth — they MUST be present after the
///     failure (replay-recoverable).
///   * Step 3 (ChainAnchorService.AnchorAsync) THROWS — the chain anchor
///     is the sole authority for governance traceability and a failure
///     must hard-stop the post-execution flow.
///   * Step 4 (OutboxService.EnqueueAsync) MUST NOT have run — without a
///     chain anchor the event is not governance-compliant and must NOT
///     be visible to external consumers.
///
/// This matches the canonical sequencing in
/// <c>src/runtime/event-fabric/EventFabric.cs</c> (persist → chain →
/// outbox) and the explicit hard-stop behavior in
/// <c>src/runtime/event-fabric/ChainAnchorService.cs</c> (engine failure
/// throws <see cref="InvalidOperationException"/>; exception
/// re-propagates; outbox never runs).
///
/// ─────────────────────────────────────────────────────────────────
/// AUDIT-FIRST ORDERING — load-bearing fact for future readers
/// ─────────────────────────────────────────────────────────────────
///
/// The runtime control plane invokes the event fabric TWICE per
/// command, in this order
/// (see <c>src/runtime/control-plane/RuntimeControlPlane.cs</c>):
///
///   1. AUDIT emission first — for the policy decision event the
///      <c>PolicyMiddleware</c> attached to <c>CommandResult.AuditEmission</c>.
///      Persisted under a SYNTHETIC audit aggregate id computed as
///      <c>IdGenerator.Generate("policy-audit-stream:{CommandId}")</c>.
///      This is NOT the command's domain aggregate id.
///
///   2. DOMAIN emission second — for the engine's emitted events,
///      persisted under the command's <c>context.AggregateId</c>.
///
/// Both pass through the same <c>EventFabric.ProcessInternalAsync</c>
/// (persist → anchor → outbox). When the chain anchor fails:
///
///   • Audit Step 2 ✅ runs    — policy decision event persisted under
///                              the audit-stream id
///   • Audit Step 3 ❌ throws  — InvalidOperationException propagates
///   • Audit Step 4 ⏭ skipped
///   • Domain ProcessAsync ⏭ NEVER REACHED — the throw aborted the
///     control plane before the domain-emission block ran
///
/// CONSEQUENCE FOR THIS TEST:
///
///   • The audit-stream aggregate id has at least one event after the
///     failure (the policy decision audit event from Step 2 of audit
///     emission). This is the "event is persisted" invariant.
///   • The DOMAIN aggregate id has ZERO events because the domain
///     emission never ran. This is the audit-first cascade —
///     stronger than the bare invariant: an audit chain failure
///     blocks not only its own outbox but the entire downstream
///     domain emission too.
///   • The outbox is empty across BOTH paths. No event reached
///     external consumers.
///
/// An earlier version of this test asserted persistence under the
/// DOMAIN aggregate id and failed because the domain emission never
/// ran. The fix recorded here corrected the test expectations to
/// reflect the audit-first ordering — the runtime behavior was
/// correct all along. The full root-cause analysis lives in the
/// session evidence record at
/// <c>claude/audits/phase1.5/evidence/5.2.6/chain-failure.evidence.md</c>.
///
/// WHY THIS USES TestHost (NOT REAL POSTGRES):
///
///   Same rationale as FR-4: the contract is the in-process Step 2 →
///   Step 3 → Step 4 ordering invariant. The in-memory adapters answer
///   "did Step 4 run?" with byte-perfect fidelity, and avoid touching
///   the shared <c>outbox</c> table. This class therefore does NOT
///   need <see cref="OutboxSharedTableCollection"/>.
/// </summary>
public sealed class ChainFailureTest
{
    [Fact]
    public async Task Chain_Anchor_Failure_Persists_Event_But_Skips_Outbox_Enqueue()
    {
        // ── Build a TestHost with a stub IChainAnchor that throws on
        // every AnchorAsync call. The override flows through
        // ChainAnchorService → EventFabric Step 3, where the throw
        // propagates and prevents Step 4 (outbox enqueue) from
        // executing — exactly the contract under test. ──
        var throwingChain = new ThrowingChainAnchor();
        var host = TestHost.ForTodo(chainAnchorOverride: throwingChain);

        var aggregateId = host.IdGenerator.Generate("fr5:agg");
        var context = host.NewTodoContext(aggregateId);
        var command = new CreateTodoCommand(aggregateId, "fr5-chain-failure-probe");

        // The audit-stream aggregate id PolicyMiddleware will use for
        // the policy decision audit event is computed deterministically
        // from the CommandId via the same IdGenerator the test uses,
        // so we can compute it here and read its event stream after
        // the failure. See the AUDIT-FIRST ORDERING block in the class
        // doc above for why this id matters.
        var auditAggregateId = host.IdGenerator.Generate(
            $"policy-audit-stream:{context.CommandId}");

        // ── The chain anchor failure throws. The exact thrown type is
        // implementation-defined (ChainAnchorService wraps engine
        // failures as InvalidOperationException; the stub here throws
        // InvalidOperationException directly so the assertion is type-
        // exact rather than catching a base Exception). ──
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await host.ControlPlane.ExecuteAsync(command, context));

        // The chain anchor was called exactly once: during the audit
        // emission. The throw aborted the control plane before the
        // domain emission could run, so the second chain call never
        // happened. Anything other than 1 here is a smoking gun for a
        // change in fabric ordering or audit emission semantics.
        Assert.Equal(1, throwingChain.CallCount);

        // ── INVARIANT 1 ("event is persisted before chain anchor"):
        // the audit emission's Step 2 ran BEFORE its Step 3 threw. The
        // policy decision audit event is in the source of truth under
        // the synthetic audit-stream aggregate id and is replay-
        // recoverable. ──
        Assert.NotEmpty(host.EventStore.AllEvents(auditAggregateId));

        // ── CASCADE INVARIANT (stronger form of "chain failure
        // hard-stops downstream"): the audit chain failure blocked the
        // domain emission entirely. The domain aggregate id has zero
        // events because Step 2 of the domain emission never ran. ──
        Assert.Empty(host.EventStore.AllEvents(aggregateId));

        // ── INVARIANT 2 ("no event published without chain anchor"):
        // Step 4 of the audit emission did NOT run, and Step 4 of the
        // domain emission was never reached either. The outbox is
        // EMPTY — no batches at all under this TestHost instance,
        // across either correlation id. ──
        Assert.Empty(host.Outbox.Batches);
    }

    /// <summary>
    /// Stub <see cref="IChainAnchor"/> that throws on every call.
    /// Counts invocations so the test can assert the chain stage
    /// actually executed (vs. being skipped by an upstream
    /// short-circuit).
    ///
    /// Throws <see cref="InvalidOperationException"/> to mirror the
    /// shape of `ChainAnchorService.AnchorAsync` engine-failure path
    /// (`throw new InvalidOperationException(...)` at line 138).
    /// </summary>
    private sealed class ThrowingChainAnchor : IChainAnchor
    {
        public int CallCount { get; private set; }

        public Task<ChainBlock> AnchorAsync(
            Guid correlationId,
            IReadOnlyList<object> events,
            string decisionHash,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            throw new InvalidOperationException(
                "FR-5 stub: simulated chain anchor outage");
        }
    }
}
