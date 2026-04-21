# Domain: Cluster

## Classification
structural-system

## Context
cluster

## Boundary
This domain defines the root cluster structure only and contains no orchestration or execution logic.

## Purpose
Defines the fundamental organizational unit within the structural system. A cluster is the root aggregate representing an organizational boundary that can be defined, activated, and archived through a terminal lifecycle.

## Core Responsibilities
- Defining new clusters with a validated identity and descriptor
- Activating defined clusters to mark them as operational
- Archiving active clusters as a terminal state

## Aggregate(s)
- **ClusterAggregate**
  - Sealed, inherits `AggregateRoot`; event tracking handled by the shared kernel
  - Factory method `Define(ClusterId, ClusterDescriptor)` for creation
  - Transition methods `Activate()` and `Archive()` with specification-guarded state transitions
  - Invariants: ClusterId must not be empty; ClusterDescriptor must have non-empty ClusterName and ClusterType

## WHEN-NEEDED folders

- no `entity/` — aggregate has no child entities.
- no `service/` — aggregate has no cross-aggregate coordination logic.

## Value Objects
- **ClusterId** — Validated Guid wrapper; rejects empty Guid
- **ClusterStatus** — Enum: Defined, Active, Archived
- **ClusterDescriptor** — Record struct with ClusterName (string, non-empty) and ClusterType (string, non-empty)

## Domain Events
- **ClusterDefinedEvent(ClusterId, ClusterDescriptor)** — Cluster created with initial descriptor
- **ClusterActivatedEvent(ClusterId)** — Cluster transitioned to Active
- **ClusterArchivedEvent(ClusterId)** — Cluster transitioned to Archived (terminal)

## Errors
- **MissingId** — ClusterId is empty Guid
- **MissingDescriptor** — ClusterDescriptor has empty ClusterName or ClusterType
- **InvalidStateTransition(status, action)** — Attempted transition not permitted from current status

## Specifications
- **CanActivateSpecification** — Status must be Defined
- **CanArchiveSpecification** — Status must be Active

## Lifecycle (TERMINAL)
```
Define() -> Defined
Activate() -> Active
Archive() -> Archived (terminal, no further transitions)
```

## Notes
- Events are sealed records with no base class
- Aggregate inherits the shared `AggregateRoot`; uncommitted events tracked by the base class
- All errors return `DomainException` (via `DomainInvariantViolationException`) through the static `ClusterErrors` factory class
