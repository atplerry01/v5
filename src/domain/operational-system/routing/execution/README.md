# Domain: Execution

## Classification
economic-system

## Context
routing

## Domain Responsibility
Represents a single execution attempt of a defined routing path. The Execution domain owns the lifecycle of an in-flight routing run — when it started, which path it targets, and how it terminated (completed, failed, or aborted). It does not define routes (that is the Path domain's responsibility) and it does not perform transport — it is the canonical, event-sourced record of a routing execution's progression.

## Aggregate
* **ExecutionAggregate** — Root aggregate representing one routing execution.
  * Private constructor; created via `Start(ExecutionId, PathId, Timestamp)` factory.
  * State transitions via `Complete()`, `Fail(reason, ...)`, `Abort(reason, ...)`.
  * Event-sourced: all state is derived from applied events.
  * Enforces invariants after every state change.

## Entities
* None

## State Model
```
Started ──Complete()──> Completed (terminal)
Started ──Fail(reason)──> Failed (terminal)
Started ──Abort(reason)──> Aborted (terminal)
```

## Value Objects
* **ExecutionId** — Deterministic identifier (validated non-empty Guid).
* **ExecutionStatus** — Enum: `Started`, `Completed`, `Failed`, `Aborted`.

## Events
* **ExecutionStartedEvent** — Raised when an execution is initiated against a path.
* **ExecutionCompletedEvent** — Raised when execution terminates successfully.
* **ExecutionFailedEvent** — Raised when execution terminates with a failure reason.
* **ExecutionAbortedEvent** — Raised when execution is cancelled with a reason.

## Invariants
* `ExecutionId` must not be empty.
* `PathId` must reference a non-empty path identifier.
* Terminal timestamp (when present) must not precede the start timestamp.
* A reason must be supplied for `Failed` and `Aborted` terminals.
* Only `Started` executions may transition to a terminal state.

## Specifications
* **CanCompleteExecutionSpecification** — Only `Started` executions can be completed.
* **CanAbortExecutionSpecification** — Only `Started` executions can be aborted.

## Errors
* **InvalidPathReference** — Start called with an empty path reference.
* **ReasonMustBeProvided** — Fail/Abort invoked without a reason.
* **InvalidStateTransition** — Guard for illegal status transitions.
* **PathIdMustNotBeEmpty** — Invariant: PathId must be non-empty.
* **TerminalBeforeStart** — Invariant: terminal timestamp must not precede start.

## Domain Services
* **ExecutionService** — Cross-aggregate checks: `CanStartOn(path)` (path must be Active); `CanTerminate(execution)` (execution must be Started).

## Lifecycle Pattern
TERMINAL — Once an execution reaches `Completed`, `Failed`, or `Aborted`, it cannot transition again.

## Boundary Statement
This domain records the execution lifecycle only. It does not define routing structure (see `path/`) and it does not perform transport, persistence, or side effects — those concerns live in the runtime and engine layers per the canonical execution flow: `API → Runtime → Engine → Domain → Event Store → Chain Anchor → Outbox → Kafka → Projection → Response`.

## Status
**S4 — Invariants + Specifications Complete**
