# Runtime Order Audit

## Purpose

Statically verify that the WBSM v3 canonical execution order is preserved
end-to-end through the runtime control plane and event fabric. This audit
produces evidence by **direct file:line citation** of the source code that
encodes the order, not by runtime tracing.

## Canonical Order (LOCKED)

The actual locked order in the codebase is **8 middlewares + 3 fabric
stages**, not the simplified 7-step order described in informal docs. Both
satisfy the WBSM v3 invariant; the audit codifies the real one.

### Middleware pipeline (request path)

Source of truth: [src/runtime/control-plane/RuntimeControlPlaneBuilder.cs:98-115](../../src/runtime/control-plane/RuntimeControlPlaneBuilder.cs#L98-L115)

```
1. TracingMiddleware           (observability)
2. MetricsMiddleware           (observability)
3. ContextGuardMiddleware      (pre-policy)   ← "Guard (pre)" in informal order
4. ValidationMiddleware        (pre-policy)   ← "Guard (pre)" in informal order
5. PolicyMiddleware            (WHYCEPOLICY)  ← "Policy"
6. AuthorizationGuardMiddleware (post-policy) ← "Guard (post-policy)"
7. IdempotencyMiddleware       (post-policy)  ← "Guard (post-policy)"
8. ExecutionGuardMiddleware    (final gate)   ← terminal pre-execution check
→ RuntimeCommandDispatcher (engine execution)
```

### Post-execution sequence (event fabric)

Source of truth: [src/runtime/event-fabric/EventFabric.cs:87-93](../../src/runtime/event-fabric/EventFabric.cs#L87-L93)

```
9.  EventStoreService.AppendAsync   ← "Persist events"
10. ChainAnchorService.AnchorAsync  ← "Chain anchoring" (MUST be after persist)
11. OutboxService.EnqueueAsync      ← "Kafka publish" (via outbox relay)
```

The comment at [EventFabric.cs:89](../../src/runtime/event-fabric/EventFabric.cs#L89) explicitly states:
"Anchor to WhyceChain (MUST happen AFTER persistence)".

## Validation Procedure

For each PASS criterion, the audit cites a specific file:line that proves it.

### Criterion 1 — All 8 middlewares are mandatory

`RuntimeControlPlaneBuilder.ValidateMandatoryDependencies()` at
[RuntimeControlPlaneBuilder.cs:118-143](../../src/runtime/control-plane/RuntimeControlPlaneBuilder.cs#L118-L143) throws
`InvalidOperationException` if any of the 8 middlewares is null. The
control plane cannot be built without all 8.

**PASS evidence:** Eight `if (_x is null) throw new InvalidOperationException(...)`
guards, one per middleware, lines 120-142.

### Criterion 2 — Order is locked at construction time, not configuration

`Build()` at [RuntimeControlPlaneBuilder.cs:98](../../src/runtime/control-plane/RuntimeControlPlaneBuilder.cs#L98) returns a
`new List<IMiddleware>` with the middlewares appended in fixed positional
order. There is no `Sort()`, no `Insert(index, ...)`, no configuration-driven
reordering anywhere in the file.

**PASS evidence:** Lines 105-115 — eight statically positioned entries,
each annotated with its sequence number in a comment. No runtime mutation
of the list is possible because `Build()` is the only constructor of the
returned list.

### Criterion 3 — Policy occurs after both pre-policy guards

In the locked order, `ContextGuardMiddleware` (line 109) and
`ValidationMiddleware` (line 110) precede `PolicyMiddleware` (line 111).
Per `IMiddleware.ExecuteAsync(context, command, next)` semantics
([src/runtime/middleware/IMiddleware.cs:11](../../src/runtime/middleware/IMiddleware.cs#L11)),
each middleware must call `next()` to advance, and the chain is built in
list order. The position in the list IS the execution order.

**PASS evidence:** Builder positions 3, 4, 5.

### Criterion 4 — Post-policy guards occur after policy and before execution

`AuthorizationGuardMiddleware` (line 112) and `IdempotencyMiddleware`
(line 113) sit between `PolicyMiddleware` (line 111) and
`ExecutionGuardMiddleware` (line 114). Execution cannot reach the
dispatcher without traversing both.

**PASS evidence:** Builder positions 6 and 7.

### Criterion 5 — Persist precedes Chain precedes Outbox

`EventFabric.ProcessAsync` is sequential `await`-based code, not parallel:

- Line 87: `await _eventStoreService.AppendAsync(...)`
- Line 90: `await _chainAnchorService.AnchorAsync(...)`
- Line 93: `await _outboxService.EnqueueAsync(...)`

There is no `Task.WhenAll`, no fire-and-forget, no `_ = ` discarded task.
Each `await` completes before the next line begins.

**PASS evidence:** [EventFabric.cs:87](../../src/runtime/event-fabric/EventFabric.cs#L87),
[EventFabric.cs:90](../../src/runtime/event-fabric/EventFabric.cs#L90),
[EventFabric.cs:93](../../src/runtime/event-fabric/EventFabric.cs#L93). All sequential `await`s
in a single method body.

### Criterion 6 — Engine execution sits between middleware and event fabric

The middleware pipeline terminates at `RuntimeCommandDispatcher`, which
invokes the engine via `engine.ExecuteAsync(engineContext)` at
[RuntimeCommandDispatcher.cs:224](../../src/runtime/dispatcher/RuntimeCommandDispatcher.cs#L224).
The dispatcher then returns the emitted events back up the chain marked
`eventsRequirePersistence: true` at
[RuntimeCommandDispatcher.cs:233](../../src/runtime/dispatcher/RuntimeCommandDispatcher.cs#L233).
The control plane then hands the events to `EventFabric.ProcessAsync` which
runs steps 9–11.

**PASS evidence:** Engine execution is the terminal step of the request
chain; event fabric is the first step of the response chain. These cannot
overlap because `dispatch` returns before `fabric` is invoked.

### Criterion 7 — No alternative entry points to the engine

Per [src/runtime/guards/RuntimeIsolationGuard.cs](../../src/runtime/guards/RuntimeIsolationGuard.cs),
[src/runtime/guards/ControlPlaneGuard.cs](../../src/runtime/guards/ControlPlaneGuard.cs), and
[src/runtime/guards/FabricInvocationGuard.cs](../../src/runtime/guards/FabricInvocationGuard.cs), the runtime
enforces at startup that no code path bypasses the control plane. These
are sister guards already in place; this audit cross-references them as
PASS evidence for the "no bypass" condition.

## Result

**STATUS: PASS**

The canonical execution order is statically auditable via direct file:line
references. All 7 criteria are satisfied by inspection. No runtime test
is required for the order property — it is encoded in the structure of the
code itself, with `ValidateMandatoryDependencies()` preventing partial
construction.

## Caveats

- This audit does **not** prove that every code path through the system
  reaches the control plane. That property is enforced by the runtime
  isolation guards (cited above), which are themselves not in scope for
  this audit.
- This audit does **not** prove that the integration test suite exercises
  the locked order. As of capture date, [tests/integration/operational-system/sandbox/todo/TodoPipelineTests.cs](../../tests/integration/operational-system/sandbox/todo/TodoPipelineTests.cs)
  does not compile (see `claude/new-rules/20260407-133419-audits.md`),
  so no test currently witnesses the order at runtime. The static evidence
  above is sufficient for the Phase 2 lock condition; runtime witness is
  a follow-up obligation.
- This audit treats `TracingMiddleware` and `MetricsMiddleware` as
  observability wrappers that do not participate in the canonical
  request/response semantics. They are listed for completeness but their
  position relative to the guards is not load-bearing for the lock.

---

## NEW CHECKS INTEGRATED — 2026-04-07

- **CHECK-RO-CANONICAL-11**: This audit canonicalizes the 11-stage order. Any prompt or doc that asserts a 7-step canonical order is treated as a human-readable summary. The audit MUST compare against RuntimeControlPlaneBuilder + EventFabric source, NOT against prompt text.
