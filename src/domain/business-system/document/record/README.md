# Domain: Record

## Classification
business-system

## Context
document

## Domain Responsibility
Manages system records through their lifecycle from active use to locking and archival. Records preserve historical integrity — once locked, they cannot be modified. Archival is only permitted after locking to ensure the record is in a stable, verified state.

## Aggregate
* **RecordAggregate** — Private constructor, static `Create(id)` factory. Manages record lifecycle with immutability enforcement after locking.

## State Model
```
[Active] --Lock()--> [Locked] --Archive()--> [Archived]
```

## Value Objects
* **RecordId** — Unique identifier (readonly record struct, Guid with validation)
* **RecordStatus** — Active | Locked | Archived

## Events
* **RecordCreatedEvent** — Raised when a new record is created
* **RecordLockedEvent** — Raised when the record is locked (immutable thereafter)
* **RecordArchivedEvent** — Raised when the record is archived

## Invariants
* RecordId must not be default
* Status must be a valid enum value
* Locked records cannot be modified
* Archival is only permitted from Locked state
* Historical integrity must be preserved

## Specifications
* **CanLockSpecification** — Only Active records can be locked
* **CanArchiveRecordSpecification** — Only Locked records can be archived
* **IsRecordModifiableSpecification** — Only Active records can be modified

## Errors
* **MissingId** — RecordId is required
* **AlreadyLocked** — Record has already been locked
* **AlreadyArchived** — Record has already been archived
* **InvalidStateTransition** — Invalid state transition attempted
* **CannotModifyLockedRecord** — Modification attempted on locked record

## Domain Services
* **RecordService** — Reserved for cross-aggregate coordination

## Status
**S4 — Invariants + Specifications Complete**
