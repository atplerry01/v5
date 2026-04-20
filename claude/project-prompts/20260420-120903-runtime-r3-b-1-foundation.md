# TITLE
R3.B.1 Foundation — Outbound Side-Effect Seam

## CONTEXT
Runtime is enterprise-grade core foundation (R1+R2+R3.A closed). Single S1 blocker to "core runtime complete" is §15 External Side-Effect Control (per audit 2026-04-20). R3.B design ratified 2026-04-20 at `claude/project-topics/v2b/closure/20260420-100946-r3-b-design.md`. R3.B.1 Step-1 design note at `claude/project-topics/v2b/closure/20260420-103811-r3-b-1-design.md` with all D-R3B1-1..9 defaults approved.

## OBJECTIVE
Ship R3.B.1 Foundation: durable-intent-first outbound seam with prohibition guards enforced from day one. Empty-but-guarded adapter whitelist (OPA grandfathered) plus a NoOp test adapter for end-to-end proof. No real provider adapters (R3.B.2), no retry hardening (R3.B.3), no webhook ingress (R3.B.4), no compensation wiring (R3.B.5).

## CONSTRAINTS
- Ratified constraints (§17.2 parent): Acknowledged never implies Finalized · provider operation identity first-class · idempotency contract mandatory · webhook/poll/manual triality designed · prohibition is law on day one.
- D-R3B1-1..9 defaults approved.
- Canonical 11-stage pipeline LOCKED; no new stages.
- R11 layer purity: runtime has zero `Whycespace.Domain.*` imports except `event-fabric/domain-schemas/**`.
- Determinism rules: `IClock`/`IIdGenerator`/`IRandomProvider` only.
- WHYCEPOLICY gates every mutation path.
- R7 persist/publish/anchor authority stays in runtime.

## EXECUTION STEPS
1. Map existing workflow parallels (aggregate, lifecycle factory, schema module, projection, bootstrap, tests).
2. Shared contracts — interfaces, records, enums.
3. Domain — aggregate, eleven lifecycle events, value objects, errors.
4. T2E lifecycle factory.
5. Runtime dispatcher + relay + options.
6. Platform — Postgres queue store adapter + migration + composition.
7. Event-fabric schema module + topic declaration.
8. Projection handler + in-memory store.
9. Guards + audit entries (18 rules).
10. Tests (unit + integration + architecture).
11. Audit sweep + gap-matrix note.

## OUTPUT FORMAT
- Files created/modified (grouped by layer).
- Guard rule IDs added/promoted.
- Migration paths.
- Test paths.
- §15 rows moved forward (ABSENT → partial-progress note).
- What remains for R3.B.2–R3.B.5.
- Maturity statement.

## VALIDATION CRITERIA
- Build clean, all tests green, 0 dependency-graph violations.
- 18 `R-OUT-EFF-*` / `DG-R-OUT-EFF-*` rules present in guards; audit probes present.
- Architecture tests prove HTTP / SDK whitelist.
- Dispatcher → queue → relay → adapter → lifecycle-event flow proven end-to-end with `NoOpOutboundEffectAdapter`.
- Post-execution $1b sweep PASS.

## CLASSIFICATION
- Layer: runtime + shared contracts + domain (integration-system/outbound-effect) + engines (T2E) + projections + platform/host
- Context: outbound-effect
- Domain: (N/A — cross-layer seam)
- Severity: S1 (architectural — closes §15 S1 blocker)
