# Domain: CommandDefinition

## Classification
core-system

## Context
command

## Domain Responsibility
Defines the structure and schema of individual commands — the canonical shape of command payloads, parameters, and metadata. This domain defines command structure only and contains no dispatch or execution logic.

## Aggregate
* **CommandDefinitionAggregate** — Root aggregate representing a command definition.
  * Private constructor; created via `Register(CommandDefinitionId, CommandSchema)` factory method.
  * State transitions via `Publish()` and `Deprecate()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.

## Entities
* None

## State Model
```
Draft ──Publish()──> Published ──Deprecate()──> Deprecated (terminal)
```

## Value Objects
* **CommandDefinitionId** — Deterministic identifier (validated non-empty Guid).
* **CommandDefinitionStatus** — Enum: `Draft`, `Published`, `Deprecated`.
* **CommandSchema** — Command name and schema version.

## Events
* **CommandDefinitionRegisteredEvent** — Raised when a new command definition is registered (status: Draft).
* **CommandDefinitionPublishedEvent** — Raised when definition is published.
* **CommandDefinitionDeprecatedEvent** — Raised when definition is deprecated (terminal).

## Invariants
* CommandDefinitionId must not be null/default.
* CommandSchema must not be null/default.
* CommandDefinitionStatus must be a defined enum value.
* Must not contain dispatch or execution logic — structure definition only.
* State transitions enforced by specifications.

## Specifications
* **CanPublishSpecification** — Only Draft definitions can be published.
* **CanDeprecateSpecification** — Only Published definitions can be deprecated.

## Errors
* **MissingId** — CommandDefinitionId is required.
* **MissingSchema** — Must define a valid schema.
* **InvalidStateTransition** — Guard for illegal status transitions.

## Domain Services
* **CommandDefinitionService** — Reserved for cross-aggregate coordination.

## Lifecycle Pattern
TERMINAL — Once deprecated, a command definition cannot be reactivated.

## Boundary Statement
This domain defines command structure only and contains no dispatch or execution logic.

## Status
**S4 — Invariants + Specifications Complete**
