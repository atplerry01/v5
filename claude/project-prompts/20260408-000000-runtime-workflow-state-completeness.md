# WBSM v3.5 — H9 WORKFLOW STATE COMPLETENESS

CLASSIFICATION: orchestration-system / workflow / state-integrity
TYPE: EVENT ENRICHMENT + REPLAY COMPLETENESS + TEST HARNESS
MODE: STRICT / DETERMINISTIC / NO-DRIFT / EVENT-SOURCED

## OBJECTIVE (LOCKED, AS PASTED)

Close the final correctness gaps in workflow execution:
1. Persist Payload at start
2. Persist Step Outputs at each step
3. Ensure Replay reconstructs full execution state
4. Establish T1M deterministic test harness

## RECONCILIATION ($5 ANTI-DRIFT)

Pasted H9 prompt overlapped with several real surfaces and invented others. Reconciled before execution:

| Pasted | Reality | Action |
|---|---|---|
| Domain event uses `WorkflowExecutionId WorkflowId` | Actual: `AggregateId AggregateId` (consistent across all 4 workflow events) | Keep `AggregateId`. Don't churn the event identity field. |
| `WorkflowStepCompletedEvent` field name `StepId` | Actual: `StepIndex`, `StepName`, `ExecutionHash`. Engine already keys `context.StepOutputs[stepDefinition.StepName]`. | Use `StepName` as the StepOutputs dictionary key. |
| Replay DTO field `WorkflowExecutionId WorkflowId` | R-DOM-01 forbids domain types in `src/runtime/**` and `src/shared/contracts/runtime/**`. | Keep `Guid WorkflowExecutionId` on the DTO. |
| `context.CurrentStepId` | Does not exist. Only `CurrentStep` (string name) and `CurrentStepIndex`. | Use `StepName` for the StepOutputs key. |
| Move `StepCompleted` emission into `WorkflowStepExecutor` | Currently emitted by `T1MWorkflowEngine` after step success. Splits responsibility for no benefit. | Skip — keep emission in engine. |
| New event factory signatures with required params | Aggregate's `Start`/`CompleteStep` (kept for replay symmetry) construct old positional events. | Use **default-null trailing params** (`object? Payload = null`) so existing aggregate constructions stay compiling. E-LIFECYCLE-FACTORY-01 is preserved — those aggregate methods are not invoked by T1M. |
| Test harness pseudocode wires `WorkflowStepExecutor()`, `WorkflowLifecycleEventFactory()`, `T1MWorkflowEngine(registry, executor, factory)` | Actual constructors: `WorkflowStepExecutor(IServiceProvider)`, `WorkflowLifecycleEventFactory()` (parameterless), `T1MWorkflowEngine(executor, factory)`. No registry parameter. | Test harness deferred (no infra exists yet); already captured in `claude/new-rules/20260407-230000-workflow-resume-payload-and-test-coverage.md`. Will extend that new-rule rather than duplicate. |
| Read model `model.StepOutputs[e.StepId] = e.Output` and `model.Payload = e.Payload` | `WorkflowExecutionReadModel` has neither field. Init-only record properties — but `Dictionary` reference can be mutated even when its init-only property is not reassigned. | Add `object? Payload` (init-only) and `Dictionary<string, object?> StepOutputs` (init-only with default new(); mutable contents). Projection handler will mutate dict + use `with` for Payload. |

### Serialization caveat (must surface)

`PostgresEventStoreAdapter.AppendEventsAsync` serializes via `JsonSerializer.Serialize(domainEvent, domainEvent.GetType())` and `LoadEventsAsync` deserializes via `EventDeserializer.DeserializeStored(eventType, payload)` to the concrete event type. After this change, an `object? Payload` field on the event will round-trip through Postgres as a `JsonElement` (since the static type on the record is `object?`). In-process / in-memory replay preserves the concrete payload reference; Postgres-backed replay returns a JsonElement that workflow steps can't blindly cast. **Typed payload resume requires a follow-up** (payload-type registry or per-event payload contracts). Captured.

## EXECUTED SCOPE

1. `WorkflowExecutionStartedEvent` — add `object? Payload = null` (trailing).
2. `WorkflowStepCompletedEvent` — add `object? Output = null` (trailing).
3. `WorkflowExecutionEventSchemas.cs` — mirror both fields on `WorkflowExecutionStartedEventSchema` and `WorkflowStepCompletedEventSchema` (nullable, trailing).
4. `WorkflowLifecycleEventFactory` — update `Started` and `StepCompleted` signatures with optional payload/output trailing params.
5. `T1MWorkflowEngine` — pass `context.Payload` into `Started(...)` and `stepResult.Output` into `StepCompleted(...)`.
6. `WorkflowExecutionBootstrap` payload mappers — pass through Payload + Output to schemas.
7. `WorkflowExecutionReadModel` — add `object? Payload` + `Dictionary<string, object?> StepOutputs` (init-only with default new()).
8. `WorkflowExecutionProjectionHandler` — populate Payload on Started; mutate `existing.StepOutputs[e.StepName] = e.Output` on StepCompleted.
9. `WorkflowExecutionReplayState` DTO — add `object? Payload` + `IReadOnlyDictionary<string, object?> StepOutputs`.
10. `WorkflowExecutionReplayService` — extract Payload from `WorkflowExecutionStartedEvent` and step outputs into a dictionary keyed on `StepName`.
11. `RuntimeCommandDispatcher.ResumeWorkflowAsync` — set `executionContext.Payload = state.Payload ?? new object()` and copy `state.StepOutputs` into `executionContext.StepOutputs`.
12. `engine.guard.md` — append `E-STATE-01..03` in 2026-04-07 NEW RULES INTEGRATED format.
13. `engine.audit.md` — append `CHECK-E-STATE-01..03`.
14. Extend `claude/new-rules/20260407-230000-...` with the typed-payload deserialization caveat and supersede the payload-on-resume gap.

## DELIBERATELY NOT EXECUTED

| Item | Reason |
|---|---|
| T1M test harness creation | Requires real `WorkflowStepExecutor` + registry + in-memory `IEventStore` + ≥2 stateless test steps; building it is bigger than the resume implementation itself. The pasted prompt's harness pseudocode also has wrong constructor signatures. Captured for follow-up in the existing new-rule. |
| Typed payload resume across Postgres | Needs a payload-type registry (event-type → payload-CLR-type map) to deserialize `object?` back to the original CLR type instead of `JsonElement`. Out of scope for H9; surfaced in new-rule. |

## VALIDATION

- E-LIFECYCLE-FACTORY-01: T1M still produces lifecycle events through the factory; aggregate methods (`Start`, `CompleteStep`, etc.) remain unused by T1M and unchanged.
- R-DOM-01: replay DTO still uses `Guid`, not `WorkflowExecutionId`. Dispatcher still references only shared contracts.
- $9 determinism: no clock, no Guid.NewGuid, no randomness; payload + outputs flow deterministically through events.
- $5 anti-drift: only additive (default-null trailing params); zero renamed types; zero new public APIs beyond the four added record fields.
- REPLAY-SENTINEL-PROTECTED-01: untouched.
