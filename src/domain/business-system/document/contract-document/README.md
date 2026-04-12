# Domain: ContractDocument

## Classification
business-system

## Context
document

## Domain Responsibility
Manages bound contract documents through their lifecycle from drafting to finalization and archival. A contract document is linked to a contract and contains structured sections. Once finalized, the document becomes immutable — no further modifications are permitted. Archival is irreversible.

## Aggregate
* **ContractDocumentAggregate** — Private constructor, static `Create(id, contractReferenceId)` factory. Manages document lifecycle and section composition. Enforces immutability after finalization.

## State Model
```
[Draft] --Finalize()--> [Finalized] --Archive()--> [Archived]
```

## Value Objects
* **ContractDocumentId** — Unique identifier (readonly record struct, Guid with validation)
* **ContractDocumentStatus** — Draft | Finalized | Archived
* **ContractReferenceId** — Reference to the linked contract (readonly record struct, Guid with validation)

## Entities
* **DocumentSection** — Structural section of the document with SectionId (Guid) and Title (string). Validated on construction.

## Events
* **ContractDocumentCreatedEvent** — Raised when a new contract document is created with a contract reference
* **ContractDocumentSectionAddedEvent** — Raised when a section is added to the document (Draft state only)
* **ContractDocumentFinalizedEvent** — Raised when the document is finalized (immutable thereafter)
* **ContractDocumentArchivedEvent** — Raised when the document is archived (irreversible)

## Invariants
* ContractDocumentId must not be default
* ContractReferenceId must not be default
* Status must be a valid enum value
* Document cannot be modified after finalization (enforced by IsModifiableSpecification)
* Archival is irreversible (only Finalized documents can be archived)

## Specifications
* **CanFinalizeSpecification** — Only Draft documents can be finalized
* **CanArchiveContractDocumentSpecification** — Only Finalized documents can be archived
* **IsModifiableSpecification** — Only Draft documents can be modified (section additions)

## Errors
* **MissingId** — ContractDocumentId is required
* **MissingContractReferenceId** — ContractReferenceId is required
* **AlreadyFinalized** — Document has already been finalized
* **AlreadyArchived** — Document has already been archived
* **InvalidStateTransition** — Invalid state transition attempted
* **CannotModifyAfterFinalization** — Modification attempted after finalization

## Domain Services
* **ContractDocumentService** — Reserved for cross-aggregate coordination

## Status
**S4 — Invariants + Specifications Complete**
