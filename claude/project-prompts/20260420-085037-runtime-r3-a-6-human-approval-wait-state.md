# TITLE
R3.A.6 — Workflow Human-Approval Wait-State (close §13 final gap)

## CONTEXT
The runtime has achieved **enterprise-grade core runtime foundation** per [runtime enterprise readiness report](../project-topics/v2b/closure/20260420-085037-runtime-enterprise-readiness-report.md). Workflow Runtime (§13) stands at 17/18 PRESENT; the single outstanding row is **human-approval wait-state**, flagged as S1 / HIGH blocking in the [closure matrix](../project-topics/v2b/closure/20260420-085037-runtime-closure-matrix.md) and in-scope per [runtime-upgrade-plan.md:215](../project-topics/v2b/runtime-upgrade-plan.md#L215).

Existing workflow infrastructure to build on (all already shipped):
- Suspend/Resume replay seam — `R-WF-RESUME-01`, `T1MWorkflowEngine`, `IWorkflowExecutionReplayService`.
- Lifecycle events — `WorkflowExecutionStartedEvent`, `WorkflowStepCompletedEvent`, `WorkflowExecutionCompletedEvent`, `WorkflowExecutionFailedEvent`, **`WorkflowExecutionSuspendedEvent`**, **`WorkflowExecutionResumedEvent`**, **`WorkflowExecutionCancelledEvent`** — all emitted via `WorkflowLifecycleEventFactory`.
- Payload/Output typed registry — `IPayloadTypeRegistry` (`R-WF-PAYLOAD-TYPED-01`).
- Workflow projection — `src/projections/orchestration/workflow/` as sole query source.

R3.A.6 is intentionally scoped as a **wait-state primitive layered on the already-shipped Suspended/Resumed/Cancelled seam** — not a parallel workflow model. The step can return an `AwaitingApproval` result that emits the canonical `WorkflowExecutionSuspendedEvent` with a structured approval signal/reason. An `ApproveWorkflowCommand` emits `WorkflowExecutionResumedEvent` and re-enters the existing resume seam; a `RejectWorkflowCommand` emits `WorkflowExecutionCancelledEvent` via the existing cancel path. Approval context (approver, decision, rationale, approval key, policy decision hash) is carried via signal/reason conventions on the existing events plus command metadata, audit evidence, and observability — **not** via new lifecycle events. Build status, test status, and dependency graph are clean at baseline (`376/376 PASS`, `0 violations`).

## OBJECTIVE
Ship a first-class human-approval wait-state so approval-governed workflows can suspend, emit a pending-approval signal, and resume or cancel deterministically in response to a governed approval decision. Moves §13 row from ABSENT to PRESENT and closes workflow-runtime completeness.

## CONSTRAINTS
- Canonical 11-stage pipeline is LOCKED per `RO-CANONICAL-11` — no reordering, no new middleware stages.
- R11 — runtime carries no domain logic; no `Whycespace.Domain.*` imports outside the sanctioned kernel exceptions.
- Determinism (GE-01) — no `Guid.NewGuid`, no `DateTime.UtcNow`; use `IIdGenerator` / `IClock` seams only.
- WHYCEPOLICY (GE-02) — Approve/Reject commands MUST pass through policy evaluation; policy decision attached to the command context.
- Event-first (GE-04) — every state transition emits a lifecycle event; no silent mutations.
- R-WF-RESUME-02: resume MUST re-enter via `T1MWorkflowEngine.ExecuteAsync` with `CurrentStepIndex` pre-populated — no parallel `ResumeAsync` API.
- R-WF-RESUME-03: the cursor is `NextStepIndex` from the replay service, not `CurrentStepIndex` from the aggregate.
- R-WF-RESUME-04/05: any `Payload`/`Output` types must be registered in `IPayloadTypeRegistry`; approval decision payload (approver, decision, rationale, approval key) MUST be registered.
- Outbox (R-K-06, R-K-13) — any approval-related event publish uses the outbox; no direct Kafka producer calls.
- R7 — persist/publish/anchor authority stays in runtime; engines emit events, runtime anchors them.
- E-STEP-02 — steps do not re-dispatch through `ISystemIntentDispatcher`; the `AwaitingApproval` result is the exit signal.
- **No new lifecycle events by default** — `WorkflowExecutionSuspendedEvent`, `WorkflowExecutionResumedEvent`, and `WorkflowExecutionCancelledEvent` are the canonical vehicles for approval state transitions. Introducing an approval-specific event family (e.g. `…AwaitingApprovalEvent`, `…ApprovedEvent`, `…RejectedEvent`) is DISALLOWED unless Step 1 proves a concrete, unavoidable insufficiency in the existing events and the user explicitly ratifies it.
- Approval context MUST be carried via the existing events' signal/reason fields, command metadata (`CorrelationId`, `CausationId`, `DecisionHash`, policy envelope), audit evidence, and observability — NOT via new event types.
- $5 Anti-drift — no unapproved architectural changes; the approval variant builds on existing primitives.
- $7 Layer purity — workflow orchestration stays in `src/engines/T1M/`; runtime handles dispatcher/outbox; domain stays pure.

## EXECUTION STEPS
1. **Design review (REUSE-FIRST sufficiency test)** — produce a short design note under `claude/project-topics/v2b/closure/` that, as its first and most important section, tests whether the existing Suspended/Resumed/Cancelled events are sufficient to carry the approval wait-state. The note MUST:
   - enumerate the data required to represent (a) entering approval wait-state, (b) approval granted, (c) approval rejected — approval key, approver, decision, rationale, policy decision hash, timeout/escalation metadata, etc.
   - map each datum onto an existing carrier: signal/reason fields on `WorkflowExecutionSuspendedEvent` / `…ResumedEvent` / `…CancelledEvent`, command metadata, the audit envelope, observability tags, or the payload registry.
   - declare the default position: no new approval-specific lifecycle events. Treat the existing triple as the canonical vehicle.
   - only if a concrete datum or invariant cannot be expressed through the reuse mapping, record the specific insufficiency (what, why, what-breaks-without-it) and propose the minimum additional surface — this requires explicit user ratification before step 2.
   - cover: shape of `WorkflowStepResult.AwaitingApproval(approvalKey, approvalPayload)`; Approve/Reject command contracts and policy binding; resume-vs-cancel branching in `T1MWorkflowEngine`; `IPayloadTypeRegistry` interaction for approval payloads; proposed new guard rules (`R-WF-APPROVAL-01…Nn`) and audit criteria — all under the reuse-first constraint.
   - capture open architectural choices as a decision log; pause for user sign-off before step 2.
2. **Contracts** — add Approve/Reject **command** contracts under `src/shared/contracts/runtime/`; register the approval-payload CLR type in the owning domain bootstrap module per `R-WF-PAYLOAD-REGISTRY-01`. **No new event contracts are added by default**; if step 1 proves an unavoidable gap, any added event contract lands under `src/shared/contracts/events/orchestration/workflow/` and carries the user's explicit ratification reference.
3. **Engine** — extend `WorkflowStepResult` with the `AwaitingApproval(approvalKey, approvalPayload)` variant; update `T1MWorkflowEngine.ExecuteAsync` to recognise and halt on it, emitting `WorkflowExecutionSuspendedEvent` via `WorkflowLifecycleEventFactory` with the approval signal/reason populated (approval key, approver role required, policy binding, any escalation hints).
4. **Runtime dispatch** — wire `ApproveWorkflowCommand` / `RejectWorkflowCommand` handlers that:
   - pass through the canonical 11-stage pipeline (policy, identity, idempotency, etc.)
   - load the workflow execution stream via the replay service; assert current state is `AwaitingApproval` via a state-transition guard
   - for Approve → emit `WorkflowExecutionResumedEvent` with the approval decision metadata in signal/reason, then re-enter the existing resume seam per `R-WF-RESUME-02..05`
   - for Reject → emit `WorkflowExecutionCancelledEvent` with the rejection decision metadata in signal/reason, via the existing cancel path.
5. **Projection** — extend `WorkflowExecutionProjectionHandler` to recognise the approval signal/reason on the existing Suspended/Resumed/Cancelled events and surface an `AwaitingApproval` / `Approved` / `Rejected` derived state on the read model; ensure replay-safety per `R-WF-PROJ-REPLAY-01`.
6. **Guard updates** — promote proposed rules into `runtime.guard.md` (Workflow section) as `R-WF-APPROVAL-01..Nn`. At minimum: (a) `AwaitingApproval` step result MUST emit `WorkflowExecutionSuspendedEvent` with the canonical approval signal shape; (b) Approve/Reject commands MUST be rejected when the execution is not in `AwaitingApproval` state; (c) new approval-specific lifecycle events are DISALLOWED unless explicitly ratified. Update `runtime.audit.md` with matching audit criteria.
7. **Tests** — add to `tests/integration/orchestration-system/workflow/`:
   - `suspend_awaits_approval_emits_pending_event`
   - `approve_resumes_from_next_step_index_no_re_execution_of_prior_steps`
   - `reject_cancels_workflow_emits_rejected_event`
   - `approve_twice_is_idempotent` (relies on R-IDEMPOTENCY-*)
   - `approve_before_suspend_is_rejected_by_policy_or_state_guard`
   - `policy_denied_approval_does_not_resume`
   - projection round-trip test asserting the three new events write correct read-model state.
8. **Gap matrix** — move the §13 human-approval row from ABSENT to PRESENT in `claude/project-topics/v2b/runtime-gap-matrix.md`.
9. **Audit sweep ($1b)** — run every `*.audit.md` under `claude/audits/`; fix any drift inline; file new findings under `claude/new-rules/` per $1c.
10. **Checkpoint commit** — a single `feat(runtime): R3.A.6 human-approval wait-state` commit covering contracts, engine, runtime handlers, projection, guards/audits, tests, and gap-matrix update.

## OUTPUT FORMAT
- Design note at `claude/project-topics/v2b/closure/{ts}-r3-a-6-design.md` (step 1) — reuse-first sufficiency test is the leading section.
- Source changes under `src/shared/contracts/runtime/` (Approve/Reject commands), `src/engines/T1M/**`, `src/runtime/**`, `src/projections/orchestration/workflow/**`. No new event contracts under `src/shared/contracts/events/**` unless step 1 proves insufficiency and user ratifies.
- Guard updates in `claude/guards/runtime.guard.md` Workflow section; audit updates in `claude/audits/runtime.audit.md`.
- New integration tests under `tests/integration/orchestration-system/workflow/`.
- Gap matrix row transition in `claude/project-topics/v2b/runtime-gap-matrix.md`.
- Post-execution sweep output captured under `claude/audits/sweeps/{ts}-r3-a-6-human-approval.md`.
- Final commit message following repo convention (`feat(runtime): R3.A.6 …`).

## VALIDATION CRITERIA
- Build clean; all tests pass (baseline `376/376` + new R3.A.6 tests); dependency graph violations = 0.
- `WorkflowStepResult.AwaitingApproval` is the only new result variant introduced; no parallel `ResumeAsync` / `ApproveAsync` API on the engine.
- Approve command drives resume through the existing `R-WF-RESUME-02..05` seam (no new resume path).
- **Lifecycle-event reuse invariant:** entering approval wait-state emits `WorkflowExecutionSuspendedEvent`; approval granted emits `WorkflowExecutionResumedEvent`; approval rejected emits `WorkflowExecutionCancelledEvent`. Zero new approval-specific lifecycle event types exist in `src/domain/**` or `src/shared/contracts/events/**` at merge time — unless step 1 proved insufficiency AND the user ratified the addition (recorded in the design-note decision log).
- Every approval-related state transition emits its lifecycle event through `WorkflowLifecycleEventFactory`; no direct event-store writes from engine code.
- Approval context (approver, decision, rationale, approval key, policy decision hash) is reachable from the read model via signal/reason fields on the existing events, command metadata, or the audit envelope — not via new event types.
- Every new command has a registered payload/discriminator per `R-WF-PAYLOAD-REGISTRY-01`.
- Approve/Reject commands carry `PolicyEvaluatedEvent`, `DecisionHash`, `ExecutionHash`, `CorrelationId`, `CausationId` per `R-PLAT-12` envelope.
- State-transition guard: Approve/Reject commands against a workflow NOT in `AwaitingApproval` state are rejected with a deterministic error; covered by test.
- Projection handler upserts on out-of-order events (replay-safety preserved).
- New guard rules cross-referenced from `runtime.audit.md` with matching audit criteria; the "no new approval-specific lifecycle events" prohibition is encoded as a guard rule.
- §13 human-approval row in gap matrix moves ABSENT → PRESENT with closure evidence cited.
- Audit sweep produces zero unresolved findings; any novel findings captured under `claude/new-rules/` per $1c.

## CLASSIFICATION
- Layer: runtime (primary), engines/T1M (workflow engine extension), projections/orchestration-system/workflow (read-model), shared/contracts (events + commands)
- Context: workflow runtime, control-plane, event-fabric
- Domain: N/A (cross-cutting runtime capability, not a bounded context)
- Severity: S1 (enterprise-claim blocking per closure matrix)
- Phase: R3.A.6 (final row of R3.A workflow reliability per runtime-upgrade-plan.md)
