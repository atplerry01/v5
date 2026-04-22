# Domain: Violation

## Classification
economic-system

## Context
enforcement

## Purpose
Records rule breaches detected during economic rule evaluation. Every violation references a rule and a source (the aggregate or event that triggered the breach). Violations follow a strict sequential lifecycle: Open -> Acknowledged -> Resolved.

## Core Responsibilities
- Recording rule breaches with rule reference and source
- Tracking violation lifecycle (acknowledgement, resolution)
- Ensuring traceability from violation to rule and source
- Requiring resolution description for closure

## Aggregate(s)
- **ViolationAggregate**
  - Event-sourced, sealed. Manages violation lifecycle from detection through resolution
  - Invariants: ViolationId cannot be empty; must reference both rule and source; cannot acknowledge non-Open violations; cannot resolve non-Acknowledged violations; cannot resolve twice; resolution requires non-empty description

## Entities
None

## Value Objects
- **ViolationId** — Typed Guid wrapper with From() factory for unique violation identity
- **ViolationStatus** — Enum: Open, Acknowledged, Resolved
- **SourceReference** — Typed Guid wrapper with From() factory for reference to triggering aggregate or event

## Domain Events
- **ViolationDetectedEvent** — Rule breach detected (captures ViolationId, RuleId, Source, Reason)
- **ViolationAcknowledgedEvent** — Violation acknowledged
- **ViolationResolvedEvent** — Violation resolved with resolution description

## Specifications
- **CanAcknowledgeSpecification** — Status == Open
- **CanResolveSpecification** — Status == Acknowledged

## Domain Services
- **ViolationService** — Stub implementation ready for domain logic
- **ViolationTraceService** — HasValidRuleReference (RuleId non-empty); HasValidSourceReference (Source non-empty)

## Invariants (CRITICAL)
- Every violation must reference a rule
- Every violation must reference a source
- Violations must not exist without a triggering condition (reason required)
- Only Open violations can be acknowledged
- Only Acknowledged violations can be resolved
- Resolution requires a non-empty description
- Resolved violations are terminal

## Policy Dependencies
- Violation traceability enforcement (must reference rule and source)
- Sequential state transition enforcement (Open -> Acknowledged -> Resolved)

## Integration Points
- **rule** — Violations reference breached rules via RuleId
- **source domain** — Violations reference triggering aggregate or event via SourceReference
- **compliance** — Violations may trigger audit evidence capture

## Lifecycle
```
Detect() -> Open (requires rule reference, source reference, reason)
  Acknowledge() -> Acknowledged
  Resolve() -> Resolved (terminal, requires resolution description)
```

## Notes
- State transitions are strictly sequential: Open -> Acknowledged -> Resolved (no skipping)
- ViolationErrors uses factory methods returning exceptions
- Both ViolationService (stub) and ViolationTraceService (implemented) exist
- RuleId is a cross-domain typed reference (not raw Guid) since it references within the same context
