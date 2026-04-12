# Domain: Evidence

## Classification
business-system

## Context
document

## Domain Responsibility
Manages immutable evidentiary proof through capture, verification, and archival. Evidence is immutable after capture — only state transitions (verification and archival) are permitted. Verification must be traceable. Designed for WhyceChain alignment and audit integrity.

## Aggregate
* **EvidenceAggregate** — Private constructor, static `Create(id)` factory. Manages evidence lifecycle with strict immutability enforcement. Artifacts can only be attached during Captured state.

## State Model
```
[Captured] --Verify()--> [Verified] --Archive()--> [Archived]
```

## Value Objects
* **EvidenceId** — Unique identifier (readonly record struct, Guid with validation)
* **EvidenceStatus** — Captured | Verified | Archived

## Entities
* **EvidenceArtifact** — Proof artifact with ArtifactId (Guid) and ArtifactType (string). Validated on construction. Immutable after attachment.

## Events
* **EvidenceCreatedEvent** — Raised when evidence is captured
* **EvidenceArtifactAttachedEvent** — Raised when an artifact is attached (Captured state only)
* **EvidenceVerifiedEvent** — Raised when evidence is verified (traceable transition)
* **EvidenceArchivedEvent** — Raised when evidence is archived

## Invariants
* EvidenceId must not be default
* Status must be a valid enum value
* Evidence is immutable after capture — no content mutation permitted
* Artifacts can only be attached during Captured state
* Verification is only permitted from Captured state
* Archival is only permitted from Verified state

## Specifications
* **CanVerifySpecification** — Only Captured evidence can be verified
* **CanArchiveEvidenceSpecification** — Only Verified evidence can be archived
* **IsEvidenceImmutableSpecification** — Evidence is immutable in all states (content cannot change)

## Errors
* **MissingId** — EvidenceId is required
* **AlreadyVerified** — Evidence has already been verified
* **AlreadyArchived** — Evidence has already been archived
* **InvalidStateTransition** — Invalid state transition attempted
* **CannotMutateAfterCapture** — Mutation attempted on immutable evidence

## Domain Services
* **EvidenceService** — Reserved for cross-aggregate coordination

## Status
**S4 — Invariants + Specifications Complete**
