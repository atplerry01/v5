# Runtime Order Guard

## Purpose

Lock the WBSM v3 canonical execution order at the source level. Any change
that reorders, removes, or makes optional any of the 11 ordered stages
(8 middlewares + 3 fabric stages) is a critical violation.

## Scope

- [src/runtime/control-plane/RuntimeControlPlaneBuilder.cs](../../src/runtime/control-plane/RuntimeControlPlaneBuilder.cs) — owns middleware order
- [src/runtime/event-fabric/EventFabric.cs](../../src/runtime/event-fabric/EventFabric.cs) — owns post-execution sequence

## Locked Order

```
REQUEST PATH (middleware pipeline)
  1. TracingMiddleware
  2. MetricsMiddleware
  3. ContextGuardMiddleware
  4. ValidationMiddleware
  5. PolicyMiddleware
  6. AuthorizationGuardMiddleware
  7. IdempotencyMiddleware
  8. ExecutionGuardMiddleware
→ RuntimeCommandDispatcher (terminal)

RESPONSE PATH (event fabric, only on success with events)
  9.  EventStoreService.AppendAsync
  10. ChainAnchorService.AnchorAsync
  11. OutboxService.EnqueueAsync
```

## Rules

1. **NO REORDERING.** The list returned by `RuntimeControlPlaneBuilder.Build()`
   must keep the 8 middlewares in positions 1–8 as listed above. Any PR that
   swaps two entries, inserts a new entry without renumbering this guard, or
   conditionally omits an entry is a violation.

2. **NO OPTIONAL MIDDLEWARES.** All 8 must remain mandatory in
   `ValidateMandatoryDependencies()`. A PR that converts a middleware to
   optional (removing its null-check) is a violation.

3. **NO PARALLEL FABRIC STAGES.** `EventFabric.ProcessAsync` must keep
   `Persist → Chain → Outbox` as three sequential `await` statements. The
   following are forbidden:
   - `Task.WhenAll(persistTask, chainTask, outboxTask)`
   - `_ = anchorTask` (fire-and-forget on chain)
   - Wrapping the three calls in a `Parallel.ForEachAsync`
   - Reordering them so chain or outbox precede persist

4. **NO ALTERNATIVE ENTRY POINTS.** Engines must only be reachable through
   the control plane. New code that calls `engine.ExecuteAsync(...)` outside
   `RuntimeCommandDispatcher` is a violation. Cross-reference with
   [runtime.guard.md](runtime.guard.md), [behavioral.guard.md rule 4](behavioral.guard.md#L19), and the runtime
   C# guards [RuntimeIsolationGuard.cs](../../src/runtime/guards/RuntimeIsolationGuard.cs),
   [ControlPlaneGuard.cs](../../src/runtime/guards/ControlPlaneGuard.cs),
   [FabricInvocationGuard.cs](../../src/runtime/guards/FabricInvocationGuard.cs).

5. **POLICY MUST BE BETWEEN PRE- AND POST-POLICY GUARDS.**
   `PolicyMiddleware` (position 5) must remain strictly after
   `ContextGuardMiddleware` + `ValidationMiddleware` (positions 3, 4) and
   strictly before `AuthorizationGuardMiddleware` + `IdempotencyMiddleware`
   (positions 6, 7). A PR that moves Policy to position 1 or position 8 is
   a violation even if all 8 middlewares are still present.

6. **CHAIN MUST FOLLOW PERSIST.** Chain anchoring may not begin until
   persistence has completed. The `await` on line 87 of `EventFabric.cs`
   must precede the `await` on line 90. A PR that hoists the chain anchor
   above the event store append is a violation.

7. **OUTBOX MUST FOLLOW CHAIN.** Outbox enqueue may not begin until chain
   anchoring has completed. The `await` on line 90 must precede the `await`
   on line 93. A PR that publishes to Kafka before the chain block is
   committed is a violation — projections would observe events that are not
   yet anchored.

## Check Procedure

1. Open `RuntimeControlPlaneBuilder.cs`. Verify the body of `Build()` matches
   the 8-entry locked sequence (see audit for the exact structural match).
2. Open `EventFabric.cs`. Verify `ProcessAsync` contains exactly three
   sequential `await` statements in the order persist → chain → outbox,
   with no `Task.WhenAll`, no discarded tasks, no conditional skips.
3. Grep `src/` for `engine.ExecuteAsync(` outside
   `src/runtime/dispatcher/`. Any hit is a violation.
4. Grep `src/` for direct calls to `IEventStore.AppendEventsAsync`,
   `IChainAnchor.AnchorAsync`, or `IOutbox.EnqueueAsync` outside
   `src/runtime/event-fabric/`. Any hit (other than the in-fabric services)
   is a violation — these surfaces must only be reached through the fabric.

## Pass Criteria

- All 8 middlewares present, mandatory, in locked order.
- Three fabric `await`s present, in locked order, sequential.
- No engine call outside the dispatcher.
- No fabric-surface call outside the fabric services.

## Fail Criteria (S0 — CRITICAL)

- Any reordering of the 8 middlewares.
- Removal or null-tolerance of any middleware.
- Parallel or reordered fabric stages.
- Engine invocation outside the dispatcher.
- Fabric surface called outside the fabric services.

## Severity

All violations of this guard are **S0 — CRITICAL**. The Phase 2 lock
condition fails until resolved. There are no S1/S2/S3 cases — the order
either holds or it does not.

## Enforcement Action

Block merge. Phase 2 lock is suspended until the order is restored. The
matching audit `claude/audits/runtime-order.audit.md` must re-PASS before
merge.

---

## NEW RULES INTEGRATED — 2026-04-07

- **RO-CANONICAL-11**: The canonical execution order is the **11-stage** order (8 middlewares + 3 fabric stages), NOT the 7-step prompt summary. Any prompt or audit that collapses to 7 steps is treated as a human-readable summary, never normative. The 11 stages are encoded structurally in RuntimeControlPlaneBuilder and EventFabric and MUST match this guard verbatim.
