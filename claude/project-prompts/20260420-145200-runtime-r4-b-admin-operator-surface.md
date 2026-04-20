# R4.B — Admin / Operator Control Surface

Classification: runtime
Context: control-plane / admin-surface
Domain: operator-controls

## TITLE
R4.B — Implement the operator/admin control surface for the runtime.

## CONTEXT
R3 closed enterprise-grade runtime correctness (workflow runtime + outbound-effect lifecycle + reconciliation + compensation + async finality). The runtime now has enough lifecycle depth that operators need safe, governed ways to inspect, control, and recover it. R4.B is operator-surface closure — not a redesign.

Prior mandate surface areas cited by the prompt:
1. outbound-effect inspection
2. reconciliation inspection + action
3. DLQ inspection
4. controlled re-drive
5. workflow execution inspection
6. pause/resume where the runtime has a real seam
7. operator audit trail
8. elevated policy/authorization boundary for admin actions

## OBJECTIVE
Deliver the minimum bounded admin/operator surface that makes operator discipline law: governed admin actions, sanctioned seams only, explicit policy gating, explicit audit evidence on every outcome branch, strict precondition gates, and architectural invariants pinned with tests + guards.

## CONSTRAINTS
- Operator actions are governed runtime actions — authorization-bound, policy-evaluated, auditable, correlation-linked; no silent operator mutation paths.
- Operator surface MUST NOT bypass canonical lifecycle seams. Controllers route through sanctioned services only.
- Admin surface MUST remain distinct from public API surface (separate route prefix, separate policy scope, separate audit coordinates).
- Every operator action must leave enough evidence to answer: who, when, why, target, outcome.
- Safety over convenience: stricter preconditions, explicit refusal classification, no silent best-effort.
- Do NOT reopen R1–R3 correctness work unless a real defect is found.
- Do NOT implement dashboarding, alert packaging, chaos, replay-certification, or broad UI in R4.B.

## EXECUTION STEPS (as delivered)
1. Recon the existing R3 runtime: identify sanctioned seams (finality service, re-drive, workflow commands), policy middleware, projection stores, and the audit-emission pattern.
2. Publish shared contracts for the admin surface: `AdminScope`, `OperatorActionEvent`, `IOperatorActionRecorder`, `IDeadLetterRedriveService` + `DeadLetterRedriveResult` + `IDeadLetterRedrivePublisher`, `IRequestCorrelationAccessor`, `OperatorActionOutcomes`.
3. Extend read-model contracts for operator inspection (bounded list queries on outbound-effect + workflow projection stores; list-all on DLQ store) with 1000-item ceilings.
4. Implement runtime services: `OperatorActionAuditRecorder` (routes via `IEventFabric.ProcessAuditAsync` with runtime operator-action classification) + `DeadLetterRedriveService` (three-gate eligibility: exists, not reprocessed, has topic + payload; publisher failure keeps row eligible).
5. Implement host adapters: `KafkaDeadLetterRedrivePublisher` (canonical UTF-8 JSON envelope; re-drive headers incl. `redrive: true`) + `HttpRequestCorrelationAccessor`.
6. Implement admin authorization: `AdminScopeRequirement` + `AdminAuthorizationHandler` + `AdminAuthorizationModule` registering the `admin-scope` policy.
7. Implement admin controllers under `src/platform/api/controllers/platform/admin/` with `AdminControllerBase`: outbound-effect (list / get / reconciliation-required / reconcile), DLQ (list / get / redrive), workflow (list / get / awaiting-approval / resume / approve / reject).
8. Compose the admin tier via `AdminCompositionModule` + `AdminCompositionModuleEntry` registered in `CompositionRegistry` at Order = 6, after the locked core→runtime→infrastructure→projections→observability sequence.
9. Tests: operator-action audit routing, DLQ re-drive eligibility across all five outcome branches, admin-surface architecture invariants (route prefix, policy gating, no domain aggregate imports, composition registration).
10. Guards: promote the seven R4.B operator-discipline rules into `runtime.guard.md` and extend `G-COMPLOAD-03` in `infrastructure.guard.md` for the admin composition order.

## OUTPUT FORMAT
See the summary response in the conversation (files created/modified, routes added, commands/seams used, guard rules added, tests added, operator actions supported, deferred items, updated runtime maturity statement).

## VALIDATION CRITERIA
- `Whycespace.Api`, `Whycespace.Runtime`, `Whycespace.Host` compile clean with zero warnings.
- All R4.B unit tests pass (15 new admin tests).
- All pre-existing architecture tests pass (including the updated `No_direct_Kafka_publish_outside_outbox_publisher` whitelist).
- `AdminSurfaceArchitectureTests` pins: every admin controller routes under `api/admin`, inherits `AdminControllerBase` or declares the admin policy, never imports `Whycespace.Domain.*`, and the admin composition module is registered in the locked registry slot.
- Guard rules R-ADMIN-SCOPE-01 / R-ADMIN-ROUTE-PREFIX-01 / R-ADMIN-NO-AGGREGATE-MUTATION-01 / R-ADMIN-AUDIT-EMISSION-01 / R-ADMIN-REDRIVE-ELIGIBILITY-01 / R-ADMIN-RECONCILE-PRECONDITION-01 / R-ADMIN-OP-IDENTITY-01 / R-ADMIN-COMPOSITION-ORDER-01 live under `claude/guards/runtime.guard.md` → R4.B section, and `G-COMPLOAD-03` in `infrastructure.guard.md` cites the Admin(6) slot.

## DEFERRED (intentional scope boundary)
- Workflow pause/resume is surfaced as RESUME only. Pause is NOT exposed because the workflow runtime does not currently have a real operator-pause seam (suspension is engine-driven); R4.B forbids inventing a fake pause endpoint.
- Schema-aware DLQ re-drive translation (evolving payloads) deferred — the current publisher preserves payload bytes verbatim; multi-version re-drive is a separate migration concern.
- Broad dashboarding / alert-rule packaging / chaos harness / replay-equivalence certification / frontend UI — explicitly out of scope for R4.B.
