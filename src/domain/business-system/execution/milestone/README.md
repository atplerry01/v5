# Domain: Milestone

## Classification
business-system

## Context
execution

## Domain Responsibility
Manages progression checkpoints within business execution flows. Milestones are defined markers that track whether an execution target reached or missed a significant checkpoint. Both Reached and Missed are terminal states — once resolved, a milestone cannot change.

## Aggregate
* **MilestoneAggregate** — Private constructor, static `Create(id, targetId)` factory. Manages milestone resolution with branching terminal states.

## State Model
```
[Defined] --Reach()--> [Reached]
           --Miss()-->  [Missed]
```

## Value Objects
* **MilestoneId** — Unique identifier (readonly record struct, Guid with validation)
* **MilestoneStatus** — Defined | Reached | Missed
* **MilestoneTargetId** — Reference to the execution target being tracked (readonly record struct, Guid with validation)

## Events
* **MilestoneCreatedEvent** — Raised when a milestone is defined with a target reference
* **MilestoneReachedEvent** — Raised when the milestone checkpoint is reached
* **MilestoneMissedEvent** — Raised when the milestone checkpoint is missed

## Invariants
* MilestoneId must not be default
* MilestoneTargetId must not be default
* Status must be a valid enum value
* Cannot reach twice (only from Defined)
* Miss only allowed from Defined state

## Specifications
* **CanReachSpecification** — Only Defined milestones can be reached
* **CanMissSpecification** — Only Defined milestones can be missed
* **IsReachedSpecification** — Checks if milestone was successfully reached

## Errors
* **MissingId** — MilestoneId is required
* **MissingTargetId** — MilestoneTargetId is required
* **InvalidStateTransition** — Invalid state transition attempted
* **AlreadyReached** — Milestone has already been reached
* **AlreadyMissed** — Milestone has already been missed

## Domain Services
* **MilestoneService** — Reserved for cross-aggregate coordination

## Status
**S4 — Invariants + Specifications Complete**
