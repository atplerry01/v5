# TITLE
R3.B.5 Compensation Emission + §15 Closure

## CONTEXT
R3.B.1–4 land: seam + first real adapter + hardening + async finality/reconciliation. 78 R3.B tests green. §15 row "Compensation or reconciliation-required signaling" is the last PARTIAL on the correctness side — the reconciliation half is operative; the compensation half ships now.

## OBJECTIVE
Emit `OutboundEffectCompensationRequestedEvent` automatically and idempotently from canonical problem outcomes, wire an example consumer pattern, and alert loudly when compensation is required but no handler is registered. Scope: additive signaling only — runtime does not perform business compensation.

## CONSTRAINTS
- Emission via `OutboundEffectLifecycleEventFactory` only; no inline event construction.
- Emission atomic with the triggering lifecycle event (single `IEventFabric.ProcessAsync` batch) — guarantees idempotency per trigger.
- Shape-based policy: `PartiallyCompleted` emits compensation only for `AtMostOnceRequired` / `CompensatableOnly` shapes. `BusinessFailed` and `RetryExhausted` emit unconditionally.
- Compensation dispatch to in-process handlers is best-effort (like the enforcement-to-policy feedback bridge pattern); handler failure must not roll back the lifecycle emission.
- Missing-handler state emits loud evidence: `outbound.effect.compensation.unhandled` counter + WARN log.

## EXECUTION STEPS
1. Shared contracts: `IOutboundEffectCompensationHandler`, `IOutboundEffectCompensationHandlerRegistry`, shape-policy helper.
2. Runtime: `OutboundEffectCompensationDispatcher` (best-effort fan-out + missing-handler signal); meter counters.
3. Finality service: emit compensation atomically with Finalized / Reconciled for qualifying outcomes.
4. Relay: emit compensation atomically with RetryExhausted.
5. Projection: handle `OutboundEffectCompensationRequestedEvent` transition → `CompensationRequested` status.
6. Composition: register default registry (DI-populated from `IEnumerable<IOutboundEffectCompensationHandler>`) + dispatcher.
7. Guards: `R-OUT-EFF-COMPENSATION-EMIT-01`, `R-OUT-EFF-COMPENSATION-ATOMIC-01`, `R-OUT-EFF-COMPENSATION-UNHANDLED-01`.
8. Tests: 4 trigger paths × 1 test each + replay non-duplication + shape policy + example handler consumption + missing-handler alert.
9. Audit sweep + gap matrix → §15 row PRESENT.

## OUTPUT FORMAT
- Files created/modified.
- Emission rules implemented.
- Example handler added.
- Tests added.
- §15 row moved to PRESENT.
- Final runtime maturity statement.

## VALIDATION CRITERIA
- Build clean, all tests green.
- Compensation emitted from all 4 qualifying outcomes.
- Replay reproduces status without duplicating events.
- Missing-handler path emits metric + log evidence.

## CLASSIFICATION
- Layer: runtime + shared contracts + projections + platform/host
- Context: outbound-effect compensation signaling
- Severity: S1 (closes the final §15 correctness row)
