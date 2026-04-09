# §5.2.6 / FR-5 — Chain Anchor Failure (EVIDENCE)

**Failure mode:** WhyceChain anchor unavailable / `IChainAnchor.AnchorAsync` throws.
**Date:** 2026-04-09
**Phase 1.5 amendment ref:** [phase1.5-reopen-amendment.md §3 §5.2.6](../../phase1.5-reopen-amendment.md)
**Test file:** [tests/integration/failure-recovery/ChainFailureTest.cs](../../../../../tests/integration/failure-recovery/ChainFailureTest.cs)
**Production seams under test:**
- [src/runtime/event-fabric/EventFabric.cs](../../../../../src/runtime/event-fabric/EventFabric.cs) — `ProcessInternalAsync` (persist → chain → outbox ordering)
- [src/runtime/event-fabric/ChainAnchorService.cs](../../../../../src/runtime/event-fabric/ChainAnchorService.cs) — chain failure hard-stop
- [src/runtime/control-plane/RuntimeControlPlane.cs](../../../../../src/runtime/control-plane/RuntimeControlPlane.cs) — audit-then-domain emission ordering
- [src/runtime/middleware/policy/PolicyMiddleware.cs](../../../../../src/runtime/middleware/policy/PolicyMiddleware.cs) — audit aggregate id derivation

---

## 1. Diagnostic outcome

**The runtime is correct. The test was checking the wrong aggregate id.**

The test expected the domain command's aggregate id to have events
persisted after a chain failure. In reality, the control plane invokes
the event fabric TWICE per command — first for the policy decision
audit emission, then for the domain emission — and both go through
the same persist → chain → outbox ordering. When the chain anchor
fails during the FIRST (audit) call, the throw aborts the control
plane before the SECOND (domain) call can run. The events DO get
persisted before the chain failure, but under the *audit-stream*
aggregate id, not the domain aggregate id the test was reading.

All four user-stated invariants are satisfied by the current runtime:

| Invariant | Status |
|---|---|
| Event is persisted (before chain anchor) | ✅ Policy decision audit event persisted under `policy-audit-stream:{CommandId}` by Step 2 of audit emission |
| Chain failure prevents outbox enqueue | ✅ Audit Step 4 skipped; domain Step 4 never reached |
| No event published without chain anchor | ✅ Outbox is empty across both audit and domain paths |
| Operation fails deterministically | ✅ `InvalidOperationException` propagates with deterministic chain-call-count of exactly 1 |

The runtime ALSO satisfies a **stronger** form of the invariant: the
audit chain failure cascades to block the domain emission entirely.
Not only is no domain event published, no domain event is even
persisted. This is the most conservative possible behavior on chain
failure and it is exactly what the Whycespace doctrine
(event store = source of truth, chain = integrity layer, outbox =
distribution layer) requires.

## 2. Root cause analysis

### 2.1 The audit-first ordering

The control plane sequence ([RuntimeControlPlane.cs:201-243](../../../../../src/runtime/control-plane/RuntimeControlPlane.cs#L201-L243)):

```
1. await pipeline(cancellationToken)         // middleware → dispatcher → engine
2. if (result.AuditEmission is not null)     // policy decision audit
       await _eventFabric.ProcessAuditAsync(audit, ...)
3. if (result.IsSuccess && events.Count > 0) // domain events
       await _eventFabric.ProcessAsync(events, ...)
```

`ProcessAuditAsync` and `ProcessAsync` both call the same internal
method `EventFabric.ProcessInternalAsync` ([EventFabric.cs:79](../../../../../src/runtime/event-fabric/EventFabric.cs#L79)) which executes the canonical fabric ordering:

```
Step 2: EventStoreService.AppendAsync       (persist)
Step 3: ChainAnchorService.AnchorAsync      (chain)
Step 4: OutboxService.EnqueueAsync          (outbox)
```

### 2.2 The audit aggregate id is synthetic and DIFFERENT from the domain id

[`PolicyMiddleware.cs:134`](../../../../../src/runtime/middleware/policy/PolicyMiddleware.cs#L134) computes the audit aggregate id as:

```csharp
var auditAggregateId = _idGenerator.Generate(
    $"policy-audit-stream:{context.CommandId}");
```

This is a deterministic synthetic id derived from the CommandId, NOT
the domain aggregate id the command operates on. The audit emission
gets persisted under this synthetic id via the
`aggregateIdOverride` parameter on
[`EventFabric.ProcessAuditAsync`](../../../../../src/runtime/event-fabric/EventFabric.cs#L66).

### 2.3 The chain failure cascade

When the chain anchor stub throws on the FIRST call:

| Stage | What runs | Outcome |
|---|---|---|
| Audit Step 2 (persist) | `EventStoreService.AppendAsync(auditAggId, [policyDecisionEvent])` | ✅ executes — audit-stream aggregate id now has 1 event |
| Audit Step 3 (chain) | `ChainAnchorService.AnchorAsync(...)` → stub throws `InvalidOperationException` | ❌ throws — propagates out of `ProcessInternalAsync` |
| Audit Step 4 (outbox) | (skipped — Step 3 threw) | ⏭ never reached |
| Control plane domain block | `if (result.IsSuccess && ...) await _eventFabric.ProcessAsync(...)` | ⏭ **never reached** — Step 3 throw aborted the control plane in the audit branch above |
| Domain Step 2 (persist) | (would have run under `context.AggregateId`) | ⏭ never reached |
| Domain Step 3 (chain) | (would have been the SECOND chain call) | ⏭ never reached |
| Domain Step 4 (outbox) | | ⏭ never reached |

The chain anchor `CallCount` after the failure is exactly **1** (the
audit emission's call), not 2. This is the fingerprint of the
audit-first cascade.

### 2.4 Why the original assertion failed

```csharp
// ORIGINAL (WRONG):
Assert.NotEmpty(host.EventStore.AllEvents(aggregateId));
//                                         ^^^^^^^^^^^
//                                         Domain aggregate id.
//                                         Domain emission never ran.
//                                         Stream has zero events.
```

The test was reading the domain aggregate id's stream and finding it
empty. That is the *correct* state for the domain stream — the domain
emission never ran because the audit emission's chain failure aborted
the control plane first. The audit-stream aggregate id, which the
test never read, has the persisted event proving Step 2 of audit
emission ran.

## 3. Decision on invariant

**The runtime stays as-is. The test is fixed to assert the correct
invariants against the audit-first ordering.**

### Why not "fix" the runtime to persist the domain events first?

Because the audit-first ordering exists for a reason: a policy
decision audit event MUST be chain-anchored before any downstream
domain event flows. If we reordered to "domain emission, then audit
emission," then a chain failure during the domain emission would
leave a domain event in the source-of-truth event store with no
corresponding audit trail in the chain — exactly the governance
violation Whycespace is built to prevent. The audit-first ordering is
load-bearing for the doctrine "no event without chain integrity."

### Why not catch the audit chain failure and continue?

Because that would mean a domain emission can occur even when the
audit emission's chain integrity failed. Same governance violation.
The whole point of the chain anchor being a hard stop is that ANY
chain failure — audit or domain — must abort the entire post-execution
flow.

The current behavior is the strictest possible reading of the
doctrine. The test was wrong to expect anything weaker.

## 4. Fix applied

[tests/integration/failure-recovery/ChainFailureTest.cs](../../../../../tests/integration/failure-recovery/ChainFailureTest.cs)
— assertions corrected to reflect the audit-first cascade. Three changes:

1. **Compute the audit-stream aggregate id deterministically** at
   the top of the test, using the same `IdGenerator.Generate(
   "policy-audit-stream:{CommandId}")` derivation that
   `PolicyMiddleware` uses. The test's `TestIdGenerator` is byte-
   identical to the production `DeterministicIdGenerator` so the
   computed id matches what the runtime persists under.

2. **Replace** `Assert.NotEmpty(host.EventStore.AllEvents(domainAggregateId))`
   **with** `Assert.NotEmpty(host.EventStore.AllEvents(auditAggregateId))`.
   This proves Step 2 of the audit emission ran (the policy decision
   event is in the source of truth) — which is the user's "event is
   persisted" invariant.

3. **Add** `Assert.Empty(host.EventStore.AllEvents(domainAggregateId))`
   to assert the cascade — the domain stream has zero events because
   the audit failure aborted the domain emission entirely. This is
   the stronger "audit chain failure cascades to block downstream
   emissions" property.

4. **Strengthen** `Assert.DoesNotContain(host.Outbox.Batches, ...)`
   (correlation-scoped) **to** `Assert.Empty(host.Outbox.Batches)`
   (instance-scoped). The TestHost has its own InMemoryOutbox per
   test invocation, so "no batches at all" is a strictly stronger
   and more honest assertion than "no batches for this correlation."

**Plus a 60-line documentation block** at the top of the test class
explaining the audit-first ordering, the cascade behavior, and a
pointer to this evidence record. Future readers will not have to
re-derive any of this.

**Zero production code modified.**

## 5. Test execution record

### ChainFailureTest in isolation

```
$ dotnet test --no-build --filter "FullyQualifiedName~ChainFailureTest"

  Passed Whycespace.Tests.Integration.FailureRecovery.ChainFailureTest
         .Chain_Anchor_Failure_Persists_Event_But_Skips_Outbox_Enqueue [91 ms]

Test Run Successful.
Total tests: 1   Passed: 1   Total time: 0.8240 Seconds
```

### Full failure-recovery suite (FR-1..FR-5 + crash recovery)

```
$ dotnet test --no-build --filter "FullyQualifiedName~FailureRecovery"

  Passed PolicyEngineFailureTest.Policy_Evaluator_Throwing_Is_Fail_Closed_With_No_Downstream_Side_Effects        [44 ms]
  Passed ChainFailureTest.Chain_Anchor_Failure_Persists_Event_But_Skips_Outbox_Enqueue                            [82 ms]
  Passed OutboxKafkaOutageRecoveryTest.Kafka_Outage_Promotes_Row_To_Deadletter_After_Retry_Budget_Exhausted       [6 s]
  Passed OutboxKafkaOutageRecoveryTest.Kafka_Recovery_Mid_Retry_Allows_Row_To_Reach_Published_State               [7 s]
  Passed PostgresFailureRecoveryTest.Connection_Drop_Mid_Batch_Rollbacks_To_Zero_Rows                             [17 ms]
  Passed PostgresFailureRecoveryTest.Recovery_After_Rollback_Reinserts_Exactly_Once                               [25 ms]
  Passed RuntimeCrashRecoveryTest.Multi_Row_Claim_Released_On_Crash_Survivors_Reprocess_All                       [57 ms]

Total: 7   Passed: 7
```

### Full integration suite

```
$ dotnet test --no-build

Passed!  - Failed: 0, Passed: 71, Skipped: 0, Total: 71,
          Duration: 14 s - Whycespace.Tests.Integration.dll (net10.0)
```

**71/71 passing. Zero failures. Zero skipped. Zero regressions
across the integration suite.**

## 6. §5.2.6 acceptance — final coverage

| Failure mode | Test | Status |
|---|---|---|
| FR-1 Kafka outage | `OutboxKafkaOutageRecoveryTest.Kafka_Outage_Promotes_Row_To_Deadletter_After_Retry_Budget_Exhausted` | ✅ |
| FR-1 Kafka recovery | `OutboxKafkaOutageRecoveryTest.Kafka_Recovery_Mid_Retry_Allows_Row_To_Reach_Published_State` | ✅ |
| FR-2 Postgres connection drop | `PostgresFailureRecoveryTest.Connection_Drop_Mid_Batch_Rollbacks_To_Zero_Rows` | ✅ |
| FR-2 Postgres recovery | `PostgresFailureRecoveryTest.Recovery_After_Rollback_Reinserts_Exactly_Once` | ✅ |
| FR-3 Policy evaluator throws | `PolicyEngineFailureTest.Policy_Evaluator_Throwing_Is_Fail_Closed_With_No_Downstream_Side_Effects` | ✅ |
| FR-5 Chain anchor failure | `ChainFailureTest.Chain_Anchor_Failure_Persists_Event_But_Skips_Outbox_Enqueue` | ✅ (this evidence) |
| Runtime crash + survivor recovery | `RuntimeCrashRecoveryTest.Multi_Row_Claim_Released_On_Crash_Survivors_Reprocess_All` | ✅ |

## 7. Status

**§5.2.6 / FR-5 Chain Anchor Failure:** ✅ **COMPLETE — EVIDENCE SIGNED.**
**§5.2.6 overall:** ✅ **COMPLETE.** All seven FR tests passing.
**Phase 1.5 re-certification:** ❌ **STILL BLOCKED** until §5.5
(Full System Multi-Instance Validation) is delivered. §5.2.6, §5.3,
and §5.4 are now all complete.
