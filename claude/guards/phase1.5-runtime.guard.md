# Phase 1.5 Runtime Guard

**STATUS: CANONICAL**
**SCOPE: §4 of phase1.5-final closure prompt**
**LOCK DATE: 2026-04-09 (pending unconditional certification — see phase1.5-final.audit.md §7)**

This guard locks the architectural and runtime invariants established by Phase 1.5 §5.2.4 (Health, Readiness, Degraded Modes) and §5.2.5 MI-1 (Distributed Execution Safety Baseline). Any future change that violates a rule below must either fail pre-execution guard validation ($1a) or be promoted via the canonical new-rules pipeline ($1c).

---

## R-RT-01 — RuntimeControlPlane is the single execution entry

**Rule**: `Whyce.Runtime.ControlPlane.RuntimeControlPlane.ExecuteAsync` is the only path that may invoke `ICommandDispatcher.DispatchAsync` or `IEventFabric.ProcessAsync` / `IEventFabric.ProcessAuditAsync`.

**Why**: Defense-in-depth. The control plane runs HSID prelude, degraded stamping, enforcement gate, the locked middleware pipeline, the policy guard, and the event fabric in a fixed order with non-bypassable invariants. Any direct call to the dispatcher or fabric bypasses these guards.

**How to apply**: New callers MUST resolve `IRuntimeControlPlane` (or `ISystemIntentDispatcher`, which forwards to it). Direct DI references to `ICommandDispatcher` or `IEventFabric` outside `RuntimeControlPlane` are forbidden.

---

## R-RT-02 — `ExecuteAsync` pipeline order is locked

**Rule**: The body of `RuntimeControlPlane.ExecuteAsync` MUST execute the following stages in this exact order:

```
1. Acquire MI-1 distributed execution lock
2. (try {) HSID v2.1 prelude
3. Stamp CommandContext.DegradedMode (HC-7)
4. RuntimeEnforcementGate.Evaluate (HC-8) — block branches return BEFORE the pipeline
5. Locked 8-middleware pipeline (Tracing → Metrics → ContextGuard → Validation → Policy → AuthorizationGuard → Idempotency → ExecutionGuard)
6. DispatchWithPolicyGuard
7. ICommandDispatcher.DispatchAsync
8. EventFabric.ProcessAuditAsync
9. EventFabric.ProcessAsync
10. (} finally { Release MI-1 lock })
```

**Why**: Each stage establishes invariants the next stage depends on. The lock must wrap everything (so a thrown exception still surrenders the lease). The HSID stamp must precede the pipeline (downstream tracing reads it). The degraded stamp must precede the enforcement gate (the gate consumes it). The enforcement gate must precede the pipeline (block decisions skip validation/policy/idempotency/engine).

**How to apply**: Re-ordering, removing, or short-circuiting any stage is a guard violation. New stages may only be inserted with an explicit guard amendment.

---

## R-RT-03 — Lock provider must be exception-free

**Rule**: `IExecutionLockProvider.TryAcquireAsync` and `IExecutionLockProvider.ReleaseAsync` MUST NOT throw on a transient store outage. The contract is exception-free; failures collapse to a deterministic `false` (acquire) or no-op (release).

**Why**: HC-9 closed this. The runtime control plane translates `false` into `execution_lock_unavailable` / `execution_cancelled` via `CommandResult.Failure(...)`. A thrown exception would surface as an unhandled 500 and break the canonical refusal vocabulary.

**How to apply**: Any new `IExecutionLockProvider` implementation MUST wrap underlying store calls in a catch-all. Verified by `ExecutionLockProviderTests.ProviderUnderRedisOutage_ReturnsFalse_NoThrow`.

---

## R-RT-04 — All §5.2.4 / §5.2.5 failures use `CommandResult.Failure(reason)`

**Rule**: Failures in the maintenance, degraded, lock, and health surfaces MUST return a structured `CommandResult.Failure` with a low-cardinality canonical reason identifier from the locked vocabulary. Exceptions MUST NOT be used as control flow at this surface.

**Why**: HC-7 / HC-8 / HC-9 / MI-1 established this discipline. Operators and audit pipelines depend on a finite, snake_case, non-payloaded reason set.

**Locked vocabulary** (additions require an amendment):
- Hard-block: `execution_lock_unavailable`, `execution_cancelled`, `system_maintenance_mode`, `restricted_during_degraded_mode`
- NotReady reasons: `host_draining`, `critical_healthcheck_failed`, `redis_unhealthy`, `postgres_pool_exhausted`, `postgres_acquisition_failures`, `postgres_invalid_pool_config`, `worker_unhealthy`, `outbox_snapshot_stale`
- Degraded reasons (= `RuntimeDegradedMode.CanonicalReasons`): `postgres_high_wait`, `opa_breaker_open`, `chain_anchor_breaker_open`, `outbox_over_high_water_mark`, `noncritical_healthcheck_failed`, `redis_degraded_latency`

**How to apply**: New reasons MUST be added to the canonical set tests (`RuntimeDegradedModeTests.CanonicalReasonSet_MatchesSpec` for the degraded set) so any future widening is a deliberate, reviewed change.

**Note**: Pre-§5.2.4 typed-exception refusal handlers (`OutboxSaturatedException`, `PolicyEvaluationUnavailableException`, etc.) remain canonical edge-handler patterns. They are NOT runtime control flow — they bubble untouched from a single throw site to a single API edge handler.

---

## R-RT-05 — Critical infrastructure dependencies must be health-checked

**Rule**: Every infrastructure dependency on the dispatch hot path MUST be exposed via an `IHealthCheck` and folded into `RuntimeStateAggregator` with a specific canonical reason. The set of currently-required dependencies is: postgres pool, workers, redis, opa, chain, outbox.

**Why**: HC-1..HC-9 closed each. The aggregator's rule chain depends on every check being present. A missing check would silently degrade the runtime's self-knowledge.

**How to apply**: Any new infrastructure dependency added on the dispatch hot path MUST come with (a) an `IHealthCheck`, (b) a specific canonical reason in the aggregator, and (c) an exclusion from the generic `critical_healthcheck_failed` and `noncritical_healthcheck_failed` scans to prevent double-counting.

---

## R-RT-06 — Determinism rules ($9) apply to all new code

**Rule**: New code MUST NOT use `Guid.NewGuid()`, `DateTime.UtcNow`, `DateTime.Now`, `DateTimeOffset.UtcNow`, `Random`, or any other non-deterministic primitive. Time MUST flow through `IClock`. IDs MUST be deterministic via `IIdGenerator` or, where genuinely process-unique infrastructure tokens are required (e.g. lock owner tokens), MUST use `{MachineName}:{ProcessId}:{Interlocked counter}` shape.

**Why**: $9 is a project-wide canonical rule. Phase 1.5 added the MI-1 owner-token shape as the canonical alternative for non-replayable process-local uniqueness needs.

**How to apply**: Pre-execution guard $1a will fail any new file containing forbidden patterns. The exemption list is the canonical clock implementation (`SystemClock.cs`) and the determinism guard documentation (`DeterminismGuard.cs`).

---

## R-RT-07 — Enforcement gate must run before the middleware pipeline

**Rule**: `RuntimeEnforcementGate.Evaluate` MUST be invoked AFTER the degraded stamp (HC-7) and BEFORE the middleware pipeline. Block decisions (`BlockMaintenance`, `BlockRestricted`) MUST return `CommandResult.Failure(...)` BEFORE any middleware runs.

**Why**: HC-8. Rejecting a maintenance request post-validation, post-policy, or post-idempotency would charge the system for work that should have been refused at the door.

**How to apply**: Any reordering that moves the enforcement gate inside or after the middleware pipeline is a guard violation. Rule order inside `RuntimeEnforcementGate.Evaluate` is also locked: maintenance dominates degraded; restricted-during-degraded dominates plain degraded.

---

## R-RT-08 — `CommandContext` write-once fields stay write-once

**Rule**: `CommandContext.PolicyDecisionAllowed`, `PolicyDecisionHash`, `Hsid`, `IdentityId`, `TrustScore`, `DegradedMode` (HC-7), and `IsExecutionRestricted` (HC-8) MUST remain write-once. Adding a free-form metadata bag is forbidden.

**Why**: Write-once invariants are the cornerstone of replay determinism and audit traceability. A free-form bag would erase the typed-field discipline and reintroduce the drift that §5.1.x closed.

**How to apply**: Any new typed field on `CommandContext` MUST follow the existing write-once pattern (private backing + setter throws on second write). New consumers MUST NOT mutate existing write-once fields after they have been stamped.

---

## R-RT-10 — Catch + rethrow allowance (observability instrumentation)

**Rule**: A `catch` clause naming a typed exception that the WBSM rules
require to "travel untouched" (currently `ConcurrencyConflictException`,
extensible) is permitted ONLY when its body is a **pure rethrow**:

- the body contains a bare `throw;` (rethrows the same exception unmodified), AND
- the body contains no `return` statement, AND
- the body contains no `throw new` (transformed exception).

Side-effecting statements before the `throw;` (e.g. histogram outcome
tagging, structured logging, metric increment) are explicitly allowed —
they do not affect exception flow.

**Why**: A pure-rethrow catch is observability instrumentation, not
control flow. The exception still travels untouched from the throw
site to the canonical edge handler. Rejecting catch+rethrow forces
authors to either (a) use exception filters (`when` clauses cannot
record outcome state without branching) or (b) duplicate observation
logic into a `finally` block, both of which are strictly worse than
the canonical pattern in `PostgresEventStoreAdapter.cs:237`.

**Counter-examples (still violations)**:

```csharp
catch (ConcurrencyConflictException) { return CommandResult.Failure(...); }   // ❌ swallows
catch (ConcurrencyConflictException ex) { throw new SomethingElse(ex); }      // ❌ transforms
catch (ConcurrencyConflictException) { /* nothing */ }                         // ❌ swallows
```

**Allowed**:

```csharp
catch (ConcurrencyConflictException) when (outcome == "ok")
{
    outcome = "concurrency_conflict";
    throw;
}
```

**How to apply**: `WbsmArchitectureTests.No_upstream_layer_catches_ConcurrencyConflictException`
enforces this via `IsPureRethrowCatchHit`. Any extension of the rule
to additional exception types MUST use the same predicate so the
allowance is uniform across the canonical "must travel untouched"
exception family.

---

## R-RT-09 — Dependency graph respects DG-R5-EXCEPT-01

**Rule**: Runtime → Host edges remain forbidden. Contracts that the runtime control plane consumes (`IRuntimeStateAggregator`, `IRuntimeMaintenanceModeProvider`, `IExecutionLockProvider`) MUST live in `Whyce.Shared.Contracts`. Concrete implementations live in `Whyce.Platform.Host`.

**Why**: §5.1.1 D1/D4 closed this. HC-7 / HC-8 / HC-9 / MI-1 all preserved it by placing every new contract in `Whyce.Shared`.

**How to apply**: New runtime-side dependencies MUST follow the contract-in-shared / concrete-in-host pattern. `bash scripts/dependency-check.sh` MUST exit 0.

---

## Lock conditions

**LOCKED 2026-04-09.** This guard is canonical and non-regressible. Phase 1.5 was unconditionally certified per `claude/audits/phase1.5/phase1.5-final.audit.md` §7 on 2026-04-09 after Patches A and B1 closed both pre-existing blockers without touching production source.

Any future workstream that needs to amend a rule above MUST:
1. Open a `claude/new-rules/{ts}-runtime.md` capture with `STATUS: PROPOSED`.
2. Reference the specific R-RT-* rule being amended.
3. Include a regression-coverage test that locks the new behavior.
4. Update this file in the same patch as the amendment.
