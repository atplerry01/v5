# Domain: Execution

## Classification
orchestration-system

## Context
workflow

## Domain Responsibility
Defines workflow execution state structure — the record of a workflow's runtime progression without containing actual execution logic. Tracks what has been executed, not how. This domain defines execution state only and contains no workflow engine or scheduling logic.

## Aggregate
* **WorkflowExecutionAggregate** — Root aggregate representing workflow execution state.
  * Created via `Start(WorkflowExecutionId, string)` factory method.
  * State transitions via `CompleteStep()`, `Complete()`, `Fail()` methods.
  * Resume handled via event factory (not in-aggregate command).
  * Event-sourced: all state derived from applied events.
  * Enforces step ordering (no skipped steps).

## Entities
* None

## State Model
```
NotStarted ──Start()──> Running ──CompleteStep()──> Running (repeatable)
Running ──Complete()──> Completed (terminal)
Running ──Fail()──> Failed ──Resume()──> Running
```

## Value Objects
* **WorkflowExecutionId** — Deterministic identifier (Guid).
* **WorkflowExecutionStatus** — Enum: `NotStarted`, `Running`, `Completed`, `Failed`.
* **ExecutionId** — Generic execution identifier (Guid).

## Events
* **WorkflowExecutionStartedEvent** — Raised when execution begins.
* **WorkflowStepCompletedEvent** — Raised when a step completes (with index, name, hash).
* **WorkflowExecutionCompletedEvent** — Raised when execution completes successfully.
* **WorkflowExecutionFailedEvent** — Raised when execution fails (with step name and reason).
* **WorkflowExecutionResumedEvent** — Raised when a failed execution is resumed.

## Invariants
* Cannot complete before started (status must be Running).
* Cannot record a step after completed.
* Cannot resume unless previously failed.
* Cannot skip steps — step index must match next expected slot.
* Workflow name must not be empty.
* Step name must not be empty.

## Specifications
* **ExecutionSpecification** — Reserved for execution validation.

## Errors
* **NotRunning** — Execution is not in Running state.
* **WorkflowNameRequired** — Workflow name is required.
* **CannotCompleteBeforeStarted** — Cannot complete before started.
* **CannotStepAfterCompleted** — Cannot record step after completion.
* **CannotResumeUnlessFailed** — Can only resume from Failed state.
* **CannotSkipSteps** — Steps must be completed in order.
* **StepNameRequired** — Step name is required.

## Domain Services
* **ExecutionService** — Reserved for cross-aggregate coordination.

## Lifecycle Pattern
SEQUENTIAL with resume — Running progresses through steps to Completed or Failed; Failed can resume.

## Boundary Statement
This domain defines execution state only and contains no workflow engine or scheduling logic.

## Status
**S4 — Invariants + Specifications Complete**
