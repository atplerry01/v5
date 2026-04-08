# WBSM v3.5 — WORKFLOW KAFKA PROJECTION WORKER + REPLAY SERVICE

CLASSIFICATION: orchestration-system / workflow / projection + recovery
TYPE: KAFKA CONSUMER + EVENT PROJECTION + REPLAY SERVICE
MODE: STRICT / EVENT-SOURCED / DETERMINISTIC / NO-DRIFT

---

## OBJECTIVE (LOCKED, AS PASTED)

Complete workflow eventification by adding:
1. Kafka-based projection consumption (production path)
2. Replay service for deterministic workflow reconstruction
3. Removal of inline projection dependency

## PROMPT BODY (VERBATIM)

The user pasted a prompt proposing the following components:

1. `src/platform/host/workers/WorkflowProjectionConsumerWorker.cs` — `BackgroundService` consuming `IKafkaConsumer` from topic `whyce.workflow.execution.events`, dispatching via `IProjectionDispatcher`.
2. `IProjectionDispatcher` / `ProjectionDispatcher` over `IEnumerable<IEnvelopeProjectionHandler>`.
3. `WorkflowStateProjectionHandler : IEnvelopeProjectionHandler` switching on `KafkaMessage.Type` for `WorkflowStartedEvent` / `WorkflowStepCompletedEvent` / `WorkflowCompletedEvent` / `WorkflowFailedEvent`, writing to `IWorkflowStateProjectionStore` with `WorkflowStateReadModel { WorkflowId, CurrentStepIndex, Status }`.
4. DI registration in `RuntimeComposition`.
5. `IWorkflowExecutionReplayService` in `src/shared/contracts/runtime/` and `WorkflowExecutionReplayService` in `src/engines/t1m/lifecycle/`, loading via `IEventStore.LoadAsync(workflowId)` and folding `WorkflowAggregate.Start/CompleteStep/Complete/Fail`.
6. New guard `G30 — WORKFLOW REPLAY` (events-only reconstruction).
7. New audit `A22 — REPLAY VALIDATION`.

## RECONCILIATION (POST-INVENTORY, $5 ANTI-DRIFT)

Inventory of the actual codebase showed the prompt overlaps with already-built or in-flight surface area. The prompt is therefore **not executed literally**. The reconciled plan executed in this session:

| Prompt item | Codebase reality | Action |
|---|---|---|
| `IKafkaConsumer` / `KafkaMessage` abstractions | Do not exist. Pattern is direct `Confluent.Kafka` use via `GenericKafkaProjectionConsumerWorker` (`src/platform/host/adapters/`). | **Skip** new abstraction. |
| `WorkflowProjectionConsumerWorker` | `GenericKafkaProjectionConsumerWorker` already covers all domains; topic wiring is per-domain via bootstrap modules (e.g. `TodoBootstrap`). | **Skip** dedicated worker. Workflow consumer wiring is owned by prompt `20260407-210000-runtime-workflow-state-eventification.md` (T-PLACEHOLDER-01) and out of scope for this prompt to avoid double-write. |
| `IProjectionDispatcher` / `ProjectionDispatcher` | `IProjectionDispatcher` already exists at `src/runtime/projection/IProjectionDispatcher.cs` with `DispatchAsync(IReadOnlyList<EventEnvelope>)`. `ProjectionRegistry` (`src/runtime/projection/ProjectionRegistry.cs`) routes by event type to `IEnvelopeProjectionHandler`. | **Skip**, already built. |
| `WorkflowStateProjectionHandler` / `WorkflowStateReadModel` / `IWorkflowStateProjectionStore` | Already exist as `WorkflowExecutionProjectionHandler` / `WorkflowExecutionReadModel` / `IWorkflowExecutionProjectionStore`. | **Skip**, already built. Naming difference is canonical and must not be churned. |
| Workflow events (`WorkflowStartedEvent` etc.) | Exist as `WorkflowExecutionStartedEvent`, `WorkflowStepCompletedEvent`, `WorkflowExecutionCompletedEvent`, `WorkflowExecutionFailedEvent` in `src/domain/orchestration-system/workflow/execution/event/`. | **Use existing names.** |
| `WorkflowAggregate` | Exists as `WorkflowExecutionAggregate` with `Start(WorkflowExecutionId, string)` / `CompleteStep(int,string,string)` / `Complete(string)` / `Fail(string,string)` and an `Apply(object)` hydrator inherited via `LoadFromHistory`. | **Use existing aggregate.** Replay must use `LoadFromHistory`, not manual fold. |
| `IEventStore.LoadAsync(workflowId)` | Actual API is `LoadEventsAsync(Guid aggregateId)`. | **Use real API.** |
| `IWorkflowExecutionReplayService` | Does not exist. Already scoped under `claude/new-rules/20260407-210000-workflow-resume-replay-service.md`. | **EXECUTE** — primary deliverable of this session. |
| `src/engines/t1m/lifecycle/` casing | Actual path is `src/engines/T1M/lifecycle/` (uppercase). | Use `T1M`. |
| `G30` workflow replay guard | `replay-determinism.guard.md` (REPLAY-SENTINEL-PROTECTED-01, REPLAY-SENTINEL-LIFT-01) and `runtime.guard.md` (R-WF-RESUME-01) already cover replay determinism. | **Skip** new guard; capture finding instead. |
| `A22` replay audit | Covered by `runtime.audit.md` CHECK R-WF-RESUME-01. | **Skip** new audit; capture finding. |
| Kafka topic name `whyce.workflow.execution.events` | Topology resolver demands `whyce.{classification}.{context}.{domain}.{type}` → canonical form is `whyce.orchestration-system.workflow.events`. | Documented; wiring deferred to the eventification prompt. |

## EXECUTED SCOPE

1. Add `IWorkflowExecutionReplayService` in `src/shared/contracts/runtime/`.
2. Implement `WorkflowExecutionReplayService` in `src/engines/T1M/lifecycle/` using `IEventStore.LoadEventsAsync` + `WorkflowExecutionAggregate.LoadFromHistory`.
3. Inject the service into `RuntimeCommandDispatcher`; replace the structured rejection of `WorkflowResumeCommand` with a real replay returning the reconstructed state.
4. Register the service in `RuntimeComposition`.
5. Capture reconciliation finding in `claude/new-rules/`.

## VALIDATION

- Replay path uses event stream only (REPLAY-SENTINEL-PROTECTED-01 unaffected; no sentinel writes).
- No `using Whycespace.Domain.*` introduced inside `src/runtime/**` (R-DOM-01).
- No new Guid.NewGuid / clock / mock data; deterministic.
- No churn of canonical names already in codebase.
