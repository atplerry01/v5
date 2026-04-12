# Domain: RegionalRule

## Classification

business-system

## Context

localization

## Domain Responsibility

Defines jurisdiction rules — the location-specific business rule set that governs operations in different regions. Rules are deterministic and define structure only. This domain defines localization structure only and contains no runtime logic.

## Aggregate

* **RegionalRuleAggregate** — Root aggregate representing a regional rule definition.
  * Private constructor; created via `Create(RegionalRuleId, RuleCode)` factory method.
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

* **RegionalRuleId** — Deterministic identifier (validated non-empty Guid).
* **RegionalRuleStatus** — Enum: `Draft`, `Active`, `Suspended`, `Archived`.
* **RuleCode** — Jurisdiction, rule type, and identifier triple.

## Events

* **RegionalRuleCreatedEvent** — Raised when a new regional rule is created (status: Draft).
* **RegionalRuleActivatedEvent** — Raised when regional rule is activated.
* **RegionalRuleSuspendedEvent** — Raised when regional rule is suspended.
* **RegionalRuleReactivatedEvent** — Raised when regional rule is reactivated from suspension.
* **RegionalRuleArchivedEvent** — Raised when regional rule is archived.

## Invariants

* RegionalRuleId must not be null/default.
* RuleCode must define jurisdiction, rule type, and identifier.
* RegionalRuleStatus must be a defined enum value.
* Rule must be deterministic — no external state dependencies.
* State transitions enforced by specifications.

## Specifications

* **CanActivateSpecification** — Only Draft regional rules can be activated.
* **CanSuspendSpecification** — Only Active regional rules can be suspended.
* **CanReactivateSpecification** — Only Suspended regional rules can be reactivated.
* **CanArchiveSpecification** — Only Active or Suspended regional rules can be archived.

## Errors

* **MissingId** — RegionalRuleId is required.
* **InvalidRuleCode** — Regional rule must define jurisdiction, rule type, and identifier.
* **InvalidStateTransition** — Generic guard for illegal status transitions.
* **DuplicateRegionalRule** — Regional rule with same jurisdiction/type/identifier already exists.

## Domain Services

* **RegionalRuleService** — Reserved for cross-aggregate coordination within regional rule context.

## Lifecycle Pattern

REVERSIBLE — Suspended regional rules can be reactivated. Archived is terminal.

## Boundary Statement

This domain defines localization structure only and contains no runtime logic.

## Status

**S4 — Invariants + Specifications Complete**
