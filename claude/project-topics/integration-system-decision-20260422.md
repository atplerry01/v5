# Integration-System Architecture Decision — 2026-04-22

## Summary

The integration-system contains exactly **one BC**: `outbound-effect/OutboundEffectAggregate`. It is **D2-complete and event-driven only**. No admin API exists or is warranted. The system operates exclusively via internal event dispatch.

---

## BC Inventory

| Context | Domain | Status | Notes |
|---------|--------|--------|-------|
| `outbound-effect` | `OutboundEffectAggregate` | **ACTIVE (D2)** | Full event-sourced lifecycle, 10 events, typed VOs, non-trivial `EnsureInvariants` |

---

## Architecture Decisions

### Decision 1: Event-Driven Only — No Admin API

**Confirmed.** The integration-system has no HTTP/command entrypoint. Construction of `OutboundEffectAggregate` is gated behind `OutboundEffectLifecycleEventFactory` (T2E seam), and an architecture test (`R-OUT-EFF-SEAM-01`) asserts that no file under `src/**` other than the factory calls `OutboundEffectAggregate.Start(`. This is a deliberate encapsulation: callers invoke the factory, not the aggregate directly.

Rationale: the integration-system models the _mechanics_ of outbound side-effects (dispatch, retry, acknowledgement, finality, reconciliation). These transitions are always triggered by upstream domain events, not user-facing commands. An admin API would bypass the seam constraint and break the isolation boundary.

### Decision 2: Single-BC Scope Is Intentional

**Confirmed.** The outbound-effect BC handles the complete outbound lifecycle. There is no inbound-effect or sync-integration BC scoped for this system. Cross-system integration is handled via events consumed by the runtime layer, not by additional integration-system BCs.

### Decision 3: Seam Pattern (R-OUT-EFF-SEAM-01 / R-OUT-EFF-SEAM-03)

Two seam rules are enforced:

- **SEAM-01**: `OutboundEffectAggregate.Start(...)` is called exclusively from `OutboundEffectLifecycleEventFactory`. Architecture test pins this.
- **SEAM-03**: There are no public mutator methods other than `Start`. State transitions occur only through domain events appended via the persist → chain → outbox pipeline. The aggregate reconstructs state on replay via `Apply`.

This means integration-system tests cannot call `aggregate.Dispatch(...)` or similar — mutation is event-append only.

### Decision 4: Idempotency Key Is Domain-Level

The `idempotencyKey` field is a first-class property on `OutboundEffectScheduledEvent` and enforced at construction. This is intentional — idempotency is a domain invariant, not an infrastructure concern. The aggregate does not deduplicate (that's a runtime concern) but it does assert that the key is non-empty at factory time.

### Decision 5: "Acknowledged ≠ Finalized" Is a Ratified Constraint

The state machine preserves six distinct adapter outcomes. `Acknowledged` → `Finalized` requires an explicit `OutboundEffectFinalizedEvent`. The `Apply` method enforces this via a Guard check. This distinction exists because acknowledged means the provider accepted the request; finalized means the provider confirmed the effect actually executed.

---

## State Machine

```
Scheduled ──dispatch──> Dispatched ──ack──────> Acknowledged ──finalize──> Finalized
    ^                      │                         │
    │               dispatch_failed             reconcile_required
    │                      │                         │
    │             (transient) TransientFailed   ReconciliationRequired ──> Reconciled
    │                      │
    └─────retry────────────┘
    
Dispatched ──dispatch_failed (terminal)──> RetryExhausted

Scheduled ──cancel──> Cancelled

Any non-terminal ──compensation_requested──> CompensationRequested
```

---

## Unit Test Status

`OutboundEffectAggregate` does **not** yet have unit tests. Adding to the Phase 1 backlog:

- `OutboundEffectAggregateTests`: Start/Schedule, Dispatch, Acknowledge, Fail (transient), RetryExhausted, Finalize, Cancel, LoadFromHistory
- Note: must invoke `Start(Guid, string, string, string, ...)` directly (not through factory) in unit tests — the architecture test SEAM-01 only restricts `src/**`, not `tests/**`

---

## No Further Architecture Work Required

The integration-system is complete at D2. It does not need:
- Additional BC promotion
- Admin API
- New contexts
- Cross-system invariant policies (it produces outbound effects; it does not consume cross-system commands)
