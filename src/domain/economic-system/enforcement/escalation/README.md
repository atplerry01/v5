# economic-system / enforcement / escalation

Stateful aggregation of violations per subject. Drives the escalation-level
signal that WHYCEPOLICY and `ExecutionGuardMiddleware` consult to progressively
restrict capital and system actions.

## Aggregate

- `ViolationEscalationAggregate` — keyed by `SubjectId`; one stream per subject.
- Events: `EscalationInitializedEvent`, `ViolationAccumulatedEvent`, `EscalationLevelIncreasedEvent`, `EscalationResetEvent`.
- Pure domain: no policy logic, no clock reads, all times arrive via `Timestamp`.

## Thresholds (domain-level classifier)

Score buckets in `EscalationThresholdSpecification`: Low≥3, Medium≥10, High≥25, Critical≥50.
Severity weights: Low=1, Medium=3, High=7, Critical=15.

These thresholds classify the counter into a level bucket so the aggregate can
emit a single categorical transition event. The *sanction* for each level is
owned by WHYCEPOLICY, not by this aggregate.

## Window

`EscalationWindow` is a 24h rolling bucket. When an incoming timestamp falls
outside the current window, the aggregate emits `EscalationResetEvent` before
the next accumulation. Windowing keeps the score bounded and replay-deterministic.

## API Surface (FAILURE-2, Option A — READ-ONLY)

**Escalation is system-driven and has no external write API.**

Writes to the escalation stream are produced exclusively by
`ViolationToEscalationHandler` (`src/runtime/event-fabric/ViolationToEscalationHandler.cs`),
which consumes `ViolationDetectedEvent` from the violation stream and dispatches
`AccumulateViolationCommand` to the T2E engine. There is no `EscalationController`
by design; external callers observe escalation state via the projection read
models at `projection_economic_enforcement_escalation.escalation_read_model`.

Rationale: escalation is an observed side-effect of violations, not a first-class
external intent. Exposing a write endpoint would let callers bypass the
violation→escalation causation chain, breaking the audit trail guaranteed by
`CausationId` propagation from `ViolationDetectedEvent`.

## Downstream Fan-Out (FAILURE-4, Option B — INTENTIONAL NON-AUTOMATION)

**Escalation does NOT automatically trigger enforcement actions.**
`EscalationLevelIncreasedEvent` is not wired to downstream sanction, lock, or
restriction commands. Downstream enforcement is external/manual/system-
orchestrated:

- **WHYCEPOLICY** reads the current escalation level as policy input (via
  `EscalationReadModel`) and denies privileged actions when thresholds are met.
  Policy denial is the primary enforcement surface, not command fan-out.
- **Operator / system orchestrator** may author explicit `IssueSanctionCommand`,
  `LockSystemCommand`, or `ApplyRestrictionCommand` citing the escalation id as
  causation. These remain authored intents under policy control.
- **No event-driven handler** in `src/runtime/event-fabric/` or
  `src/engines/T2E/economic/enforcement/` consumes
  `EscalationLevelIncreasedEvent` to produce an enforcement command.

Rationale: translating an escalation-level transition into a specific sanction
type, lock scope, or restriction scope requires policy/threshold design that
lives in WHYCEPOLICY, not in domain event wiring. Implicit fan-out would (a) bake
policy into the event fabric, violating POL-10 (separation of policy and domain
rules), and (b) remove the mandatory `ActorId` on the resulting enforcement
command, violating INV-201 (Mandatory Actor Context).

If automated fan-out is ever required, the canonical path is a new T4G-style
policy-evaluator engine subscribed to `EscalationLevelIncreasedEvent`, with its
own declared policy id and explicit service-identity actor. Until then, the
absence of an event handler is intentional and guarded by this README clause.
