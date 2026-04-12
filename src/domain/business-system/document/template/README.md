# Domain: Template

## Maturity Level

S4 — Invariants + Specifications Complete

## Classification

business-system

## Context

document

## Purpose

Defines the structure of document templates — reusable document patterns for generating standardised business documents. Manages the full template lifecycle from draft through publication to deprecation.

## Core Responsibilities

* Define template structure and metadata
* Track template identity and classification
* Maintain template lifecycle state (Draft -> Published -> Deprecated)
* Enforce immutability after publication
* Ensure deprecation does not break existing usage

## Aggregate(s)

* TemplateAggregate

  * Manages the lifecycle and integrity of a template entity
  * State model: Draft -> Published -> Deprecated
  * Enforces invariants: TemplateId must be valid, status must be a defined enum value
  * Supports event sourcing via uncommitted events
  * Structures can only be added while in Draft status

## Entities

* TemplateStructure — Structure definition for a template section (SectionId, Name, Definition)

## Value Objects

* TemplateId — Unique identifier for a template instance (guards against Guid.Empty)
* TemplateStatus — Enum representing lifecycle state: Draft, Published, Deprecated

## Domain Events

* TemplateCreatedEvent(TemplateId) — Raised when a new template is created
* TemplatePublishedEvent(TemplateId) — Raised when a template transitions to Published
* TemplateDeprecatedEvent(TemplateId) — Raised when a template transitions to Deprecated

## Specifications

* CanPublishSpecification — Validates that a template is in Draft status before publishing
* CanDeprecateSpecification — Validates that a template is in Published status before deprecating
* IsPublishedSpecification — Checks whether a template is currently in Published status

## Errors

* TemplateDomainException — Domain-specific exception type
* TemplateErrors — Static factory for domain errors:
  * MissingId — TemplateId is required and must not be empty
  * InvalidStateTransition — Attempted action not allowed in current status
  * StructureRequired — Template must contain at least one structure definition before publishing
  * ModificationAfterPublish — Published templates cannot be modified

## Domain Services

* TemplateService — Domain operations for template management (placeholder)

## Invariants

* TemplateId must not be empty (Guid.Empty rejected)
* TemplateStatus must be a defined enum value
* Published templates cannot be modified (immutability after publish)
* Templates must have at least one TemplateStructure to be published
* State transitions are unidirectional: Draft -> Published -> Deprecated

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY
* ValidateBeforeChange provides a policy hook enforced by runtime

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Lifecycle

Draft -> Published -> Deprecated

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed. No Guid.NewGuid(), no DateTime.UtcNow, no external dependencies in domain layer. All classes are sealed.
