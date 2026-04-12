# Domain: Template

## Classification

business-system

## Context

notification

## Domain Responsibility

Defines message structure — templates that define the shape and content of notifications. Templates are immutable after publication. This domain defines notification structure only and contains no execution logic.

## Aggregate

* **TemplateAggregate** — Root aggregate representing a notification template definition.
  * Private constructor; created via `Create(TemplateId, TemplateContent)` factory method.
  * State transitions via `Publish()` and `Archive()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.
  * Immutable after publication — content cannot be modified once published.

## Entities

* None

## State Model

```
Draft ──Publish()──> Published ──Archive()──> Archived (terminal)
```

## Value Objects

* **TemplateId** — Deterministic identifier (validated non-empty Guid).
* **TemplateStatus** — Enum: `Draft`, `Published`, `Archived`.
* **TemplateContent** — Subject and body defining the message structure.

## Events

* **TemplateDraftedEvent** — Raised when a new template is drafted (status: Draft).
* **TemplatePublishedEvent** — Raised when template is published (becomes immutable).
* **TemplateArchivedEvent** — Raised when template is archived (terminal).

## Invariants

* TemplateId must not be null/default.
* TemplateContent must define subject and body.
* TemplateStatus must be a defined enum value.
* Template content is immutable after publication.
* Must not render or send messages — structure definition only.
* State transitions enforced by specifications.

## Specifications

* **CanPublishSpecification** — Only Draft templates can be published.
* **CanArchiveSpecification** — Only Published templates can be archived.

## Errors

* **MissingId** — TemplateId is required.
* **InvalidContent** — Template must define valid subject and body content.
* **InvalidStateTransition** — Guard for illegal status transitions.
* **ImmutableAfterPublish** — Template content cannot be modified after publication.

## Domain Services

* **TemplateService** — Reserved for cross-aggregate coordination within template context.

## Lifecycle Pattern

TERMINAL — Once archived, a template cannot be reactivated. Content is immutable after publication.

## Boundary Statement

This domain defines notification structure only and contains no execution logic.

## Status

**S4 — Invariants + Specifications Complete**
