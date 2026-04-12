# Domain: Kanban

## Classification
operational-system

## Context
sandbox

## Domain Responsibility
Defines kanban board and card structure — boards containing lists of cards with positioning, priority, and completion state. This domain defines board structure only and contains no task execution or scheduling logic.

## Aggregate
* **KanbanAggregate** — Root aggregate representing a kanban board.
  * Created via `Create(KanbanBoardId, string)` factory method.
  * Manages lists and cards via `CreateList()`, `CreateCard()`, `MoveCard()`, `ReorderCard()`, `CompleteCard()`, `ReviseCardDetail()`.
  * Event-sourced: all state derived from applied events.

## Entities
* **KanbanList** — A list within the board containing cards with position management.
* **KanbanCard** — An individual card with title, description, position, priority, and completion state.

## State Model
```
Board: Created (permanent)
Card: Active ──Complete()──> Completed (terminal)
```

## Value Objects
* **KanbanBoardId** — Board identifier.
* **KanbanListId** — List identifier.
* **KanbanCardId** — Card identifier.
* **KanbanPosition** — Integer position within a list.
* **KanbanStatus** — Enum: Backlog, Todo, InProgress, Review, Done.
* **KanbanPriority** — Enum: Low, Medium, High, Critical.

## Events
* **KanbanBoardCreatedEvent** — Raised when board is created.
* **KanbanListCreatedEvent** — Raised when a list is added.
* **KanbanCardCreatedEvent** — Raised when a card is added.
* **KanbanCardMovedEvent** — Raised when a card is moved between lists.
* **KanbanCardReorderedEvent** — Raised when a card is reordered within a list.
* **KanbanCardCompletedEvent** — Raised when a card is completed.
* **KanbanCardDetailRevisedEvent** — Raised when card details are revised.

## Invariants
* Board name must not be empty.
* List names must not be empty; positions must be non-negative.
* No duplicate list IDs or positions.
* Card titles must not be empty; completed cards cannot be modified.
* Cards can only be moved to existing lists.

## Errors
* 11 domain error messages covering board, list, and card validations.

## Domain Services
* None

## Lifecycle Pattern
Board is permanent. Cards are TERMINAL once completed.

## Boundary Statement
This domain defines board structure only and contains no task execution or scheduling logic.

## Status
**S4 — Invariants + Specifications Complete**
