# Domain: Translation

## Classification

business-system

## Context

localization

## Domain Responsibility

Defines text mapping structure — the source-to-target language key that represents a translatable content entry. This domain defines localization structure only and contains no runtime logic.

## Aggregate

* **TranslationAggregate** — Root aggregate representing a translation mapping definition.
  * Private constructor; created via `Create(TranslationId, TranslationKey)` factory method.
  * State transitions via `Activate()`, `Suspend()`, `Reactivate()`, and `Archive()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## Entities

* None

## State Model

```
Draft ──Activate()──> Active ──Suspend()──> Suspended ──Reactivate()──> Active
                      Active ──Archive()──> Archived
                                Suspended ──Archive()──> Archived
```

## Value Objects

* **TranslationId** — Deterministic identifier (validated non-empty Guid).
* **TranslationStatus** — Enum: `Draft`, `Active`, `Suspended`, `Archived`.
* **TranslationKey** — Source language, target language, and key identifier.

## Events

* **TranslationCreatedEvent** — Raised when a new translation is created (status: Draft).
* **TranslationActivatedEvent** — Raised when translation is activated.
* **TranslationSuspendedEvent** — Raised when translation is suspended.
* **TranslationReactivatedEvent** — Raised when translation is reactivated from suspension.
* **TranslationArchivedEvent** — Raised when translation is archived.

## Invariants

* TranslationId must not be null/default.
* TranslationKey must define source language, target language, and key.
* TranslationStatus must be a defined enum value.
* Must not perform translation — mapping structure definition only.
* State transitions enforced by specifications.

## Specifications

* **CanActivateSpecification** — Only Draft translations can be activated.
* **CanSuspendSpecification** — Only Active translations can be suspended.
* **CanReactivateSpecification** — Only Suspended translations can be reactivated.
* **CanArchiveSpecification** — Only Active or Suspended translations can be archived.

## Errors

* **MissingId** — TranslationId is required.
* **InvalidTranslationKey** — Translation must define source language, target language, and key.
* **InvalidStateTransition** — Generic guard for illegal status transitions.
* **DuplicateTranslation** — Translation with same source/target/key already exists.

## Domain Services

* **TranslationService** — Reserved for cross-aggregate coordination within translation context.

## Lifecycle Pattern

REVERSIBLE — Suspended translations can be reactivated. Archived is terminal.

## Boundary Statement

This domain defines localization structure only and contains no runtime logic.

## Status

**S4 — Invariants + Specifications Complete**
