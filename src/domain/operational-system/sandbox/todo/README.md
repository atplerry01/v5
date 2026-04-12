# Domain: Todo

## Classification
operational-system

## Context
sandbox

## Domain Responsibility
Defines task structure — the representation of simple to-do items with title and completion state. This domain defines task structure only and contains no task execution or scheduling logic.

## Aggregate
* **TodoAggregate** — Root aggregate representing a to-do item.
  * Created via `Create(TodoId, string)` factory method.
  * State transitions via `ReviseTitle()` and `Complete()` methods.
  * Event-sourced: all state derived from applied events.

## Entities
* None

## State Model
```
Created ──ReviseTitle()──> Created (repeatable)
Created ──Complete()────> Completed (terminal)
```

## Value Objects
* **TodoId** — Deterministic identifier (Guid).

## Events
* **TodoCreatedEvent** — Raised when a new todo is created.
* **TodoTitleRevisedEvent** — Raised when todo title is revised.
* **TodoCompletedEvent** — Raised when todo is completed (terminal).

## Invariants
* Title must not be empty.
* Completed todos cannot be revised.
* Cannot complete an already-completed todo.

## Specifications
* **CanReviseTitleSpecification** — Only non-completed todos can be revised.
* **CanCompleteSpecification** — Only non-completed todos can be completed.

## Errors
* **TitleRequired** — Todo title is required.
* **CannotUpdateCompleted** — Cannot update a completed todo.
* **AlreadyCompleted** — Todo is already completed.

## Domain Services
* **TodoService** — Reserved for cross-aggregate coordination.

## Lifecycle Pattern
TERMINAL — Once completed, a todo cannot be modified.

## Boundary Statement
This domain defines task structure only and contains no task execution or scheduling logic.

## Status
**S4 — Invariants + Specifications Complete**
