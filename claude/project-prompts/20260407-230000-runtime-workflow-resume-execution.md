# WBSM v3.5 — H8 WORKFLOW RESUME EXECUTION

CLASSIFICATION: orchestration-system / workflow / execution
TYPE: ENGINE EXTENSION + DISPATCHER INTEGRATION + RESUME LOGIC
MODE: STRICT / DETERMINISTIC / EVENT-SOURCED / NO-DRIFT

## OBJECTIVE (LOCKED, AS PASTED)

Enable true workflow continuation from replayed state:
- Replay reconstructs aggregate (already done in 20260407-220000)
- Engine resumes execution from correct step
- Events emitted via factory → sink → runtime pipeline
- NO state mutation outside domain
- NO re-execution of completed steps

## RECONCILIATION ($5 ANTI-DRIFT)

Inventory of `T1MWorkflowEngine` revealed that **resume support already exists implicitly**:

- `T1MWorkflowEngine.ExecuteAsync` reads `var startIndex = context.CurrentStepIndex;` (`WorkflowEngine.cs:37`).
- It only emits `Started` when `startIndex == 0` (`WorkflowEngine.cs:39`).
- The loop `for (var i = startIndex; i < definition.Steps.Count; i++)` resumes from any cursor.
- Lifecycle events are produced via `WorkflowLifecycleEventFactory` (E-LIFECYCLE-FACTORY-01).

The pasted prompt proposes a separate `IWorkflowEngine.ResumeAsync(aggregate, context, ct)` method that would invent a parallel execution path, duplicate the loop body, take a domain aggregate across the runtime boundary (violating R-DOM-01 if dispatcher passes it), and is unnecessary.

Additional drift in the pasted prompt:
| Pasted | Reality |
|---|---|
| `WorkflowExecutionEngine` | Actual class is `T1MWorkflowEngine` |
| `_registry.GetSteps(name)` | Actual API: `IWorkflowRegistry.Resolve(name)` returning `IReadOnlyList<Type>?` |
| `_eventFactory.StepCompleted(workflowId, index)` (2 params) | Actual: `StepCompleted(Guid, int, string, string)` |
| `_eventFactory.WorkflowCompleted(workflowId)` | Actual: `Completed(Guid, string executionHash)` |
| `_eventFactory.WorkflowFailed(workflowId, error)` | Actual: `Failed(Guid, string failedStepName, string reason)` |
| `aggregate.WorkflowId` | Actual: `aggregate.Id : WorkflowExecutionId` (struct), use `.Id.Value` |
| `CommandResult.Success()` no-arg | Actual: `Success(IReadOnlyList<object> events, …)` |
| `_replayService.ReplayAsync(resume.WorkflowId)` returns aggregate | Actual: returns `WorkflowExecutionReplayState?` DTO (per R-DOM-01 — runtime cannot see aggregates) |
| Step continuation rule "starts at CurrentStepIndex" | Aggregate semantics: `CurrentStepIndex = LAST COMPLETED step`. Engine context semantics: `CurrentStepIndex = NEXT step to run`. Resume cursor must be **last_completed + 1**, BUT aggregate is ambiguous between "started, no steps done" and "step 0 completed". Resolved by exposing `NextStepIndex` from replay derived from the count of `WorkflowStepCompletedEvent` instances in the stream. |
| New `IWorkflowEngine.ResumeAsync` | Not needed — existing `ExecuteAsync` already honors `context.CurrentStepIndex` as resume cursor and gates `Started` emission. |

## EXECUTED SCOPE

1. **Replay DTO** (`IWorkflowExecutionReplayService.cs`): replace ambiguous `CurrentStepIndex` with `NextStepIndex` (the cursor the engine consumes). DTO is internal to dispatcher↔replay; no external consumers.
2. **Replay impl** (`WorkflowExecutionReplayService.cs`): compute `NextStepIndex` as count of `WorkflowStepCompletedEvent` instances in the loaded stream — unambiguous and event-derived.
3. **Dispatcher** (`RuntimeCommandDispatcher.cs`):
   - Extract `BuildDefinition(workflowName)` private helper used by both start and resume paths so step IDs are guaranteed identical (start computed `ComputeStepId(workflowName, stepName, index)` — resume MUST match).
   - Replace the read-only resume path with: replay → guard `Status == "Running"` → reconstruct definition → build context with `CurrentStepIndex = state.NextStepIndex` and `ExecutionHash = state.ExecutionHash` → call `_workflowEngine.ExecuteAsync` → wrap result in `CommandResult` exactly as the start path does.
4. **Guards/audits**: append `E-RESUME-01..03` to `claude/guards/engine.guard.md` and `CHECK-E-RESUME-01..02` to `claude/audits/engine.audit.md` (matches existing 2026-04-07 NEW RULES INTEGRATED format).

## DELIBERATELY NOT EXECUTED

| Pasted item | Reason | Follow-up |
|---|---|---|
| `IWorkflowEngine.ResumeAsync` overload | Existing `ExecuteAsync` already supports resume via `context.CurrentStepIndex`. Adding a parallel method would duplicate the loop body and bifurcate the lifecycle-event emission contract (E-LIFECYCLE-FACTORY-01). | None — existing API is sufficient. |
| Resume `Payload` restoration | `WorkflowExecutionStartedEvent` carries only `WorkflowName`, not the original payload. Steps that depend on payload cannot be resumed without persisting payload. | New-rule capture: payload-on-resume policy (defer or persist payload in started event). |
| Resume `StepOutputs` restoration | Step outputs are not currently persisted; only `ExecutionHash` is reconstructable. Steps that depend on prior step outputs at runtime cannot be resumed. | Same new-rule. |
| Test scenarios 1-3 (Resume Midway / Completed / Failure) | `tests/unit/` and `tests/integration/` have **no existing T1M happy-path harness**. Building one in this session would require wiring a real `WorkflowStepExecutor`, real registry, real in-memory event store, and at least two stateless test steps — meaningful test infra work outside the resume implementation itself. | New-rule capture: `T1M-RESUME-TEST-COVERAGE-01`. |

## VALIDATION

- E-LIFECYCLE-FACTORY-01: replay still uses `LoadFromHistory` (Apply path); dispatcher resume path emits events via the factory through the existing engine. No direct aggregate mutation introduced.
- R-DOM-01: dispatcher only references `IWorkflowExecutionReplayService` + `WorkflowExecutionReplayState` (shared contracts). No `using Whycespace.Domain.*` added under `src/runtime/**`.
- REPLAY-SENTINEL-PROTECTED-01: `EventReplayService.cs:57-59` sentinels untouched.
- Determinism ($9): no `DateTime.UtcNow`, no `Guid.NewGuid`, no randomness; resume cursor derived deterministically from event count.
- Anti-drift ($5): no parallel APIs introduced; existing names used everywhere.
