# Domain: SignatureRecord

## Maturity Level

S4 — Invariants + Specifications Complete

## Classification

business-system

## Context

document

## Purpose

Defines the structure of signature records — the documentary evidence of signatures applied to business documents. Manages the lifecycle of audit signature records through event-sourced state transitions: Captured, Verified, Archived.

## Core Responsibilities

* Define signature record structure and metadata
* Track signature record identity and classification
* Maintain signature record lifecycle state (Captured -> Verified -> Archived)
* Enforce immutability after verification
* Ensure at least one signature entry exists before verification

## Aggregate(s)

* SignatureRecordAggregate
  * Event-sourced aggregate managing the lifecycle and integrity of a signature record
  * Private constructor with static `Create` factory method
  * Tracks uncommitted events and version
  * Methods: `Create`, `AddEntry`, `Verify`, `Archive`
  * `EnsureInvariants()` validates Id != default and Enum.IsDefined(Status)

## Entities

* SignatureEntry — Signature metadata including entry ID, signer reference, source document reference, and signature hash

## Value Objects

* SignatureRecordId — Strongly-typed identifier (`readonly record struct`) with Guid.Empty validation
* SignatureRecordStatus — Enum: Captured, Verified, Archived

## Domain Events

* SignatureRecordCreatedEvent(SignatureRecordId) — Raised when a new signature record is created
* SignatureRecordVerifiedEvent(SignatureRecordId) — Raised when a signature record is verified
* SignatureRecordArchivedEvent(SignatureRecordId) — Raised when a signature record is archived

## Specifications

* CanVerifySpecification — Validates that status is Captured before allowing verification
* CanArchiveSpecification — Validates that status is Verified before allowing archival
* IsVerifiedSpecification — Returns true if status is Verified or Archived (post-verification states)

## Domain Errors

* SignatureRecordErrors — Static factory methods for domain-specific exceptions
  * MissingId — SignatureRecordId is required and must not be empty
  * InvalidStateTransition — Cannot perform action in current status
  * SignatureEntryRequired — Must have at least one entry before verification
  * ModificationAfterVerification — Cannot modify after verification
* SignatureRecordDomainException — Sealed domain exception type

## Domain Services

* SignatureRecordService — Domain operations for signature record management (placeholder)

## Invariants

* SignatureRecordId must not be default/empty
* Status must be a defined enum value
* Cannot modify (add entries) after Verified or Archived state
* At least one SignatureEntry required before verification
* State transitions: Captured -> Verified -> Archived (no skipping, no reversal)

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY
* ValidateBeforeChange provides a policy hook enforced by runtime

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Lifecycle

Captured -> Verified -> Archived

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed. Domain layer has zero external dependencies.
