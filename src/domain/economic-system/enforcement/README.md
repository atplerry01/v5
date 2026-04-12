# Domain: Enforcement

## Classification
economic-system

## Context
enforcement

## Purpose
Economic control layer responsible for representing enforceable rules and tracking violations when those rules are breached. Enforcement does not mutate financial truth — it identifies and tracks breaches.

## Core Responsibilities
- Defining enforceable economic rules with scope, category, and severity
- Managing rule lifecycle (Active, Disabled, Retired)
- Recording rule breaches as violations
- Tracking violation lifecycle (Open -> Acknowledged -> Resolved)
- Ensuring traceability from violation to rule and source

## Aggregate(s)
- **EnforcementRuleAggregate** (`rule/`)
  - Defines enforceable economic rules with scope, category, and severity
  - Invariants: Must have non-empty name; cannot retire a Retired rule; cannot disable a non-Active rule; cannot activate a Retired rule

- **ViolationAggregate** (`violation/`)
  - Records rule breaches with rule reference, source, and reason
  - Invariants: Must reference a rule and source; reason required; sequential state transitions only (Open -> Acknowledged -> Resolved)

## Entities
None

## Value Objects
- **RuleId** — Typed Guid wrapper with From() factory for unique rule identity
- **RuleCode** — Machine-readable rule code (string, cannot be empty)
- **RuleCategory** — Rule categorization (string, cannot be empty)
- **RuleScope** — Enum: Capital, Ledger, Transaction, Revenue, Exposure, Reconciliation
- **RuleSeverity** — Enum: Low, Medium, High, Critical
- **RuleStatus** — Enum: Active, Disabled, Retired
- **ViolationId** — Typed Guid wrapper with From() factory for unique violation identity
- **ViolationStatus** — Enum: Open, Acknowledged, Resolved
- **SourceReference** — Typed Guid wrapper with From() factory for source aggregate/event reference

## Domain Events
- **EnforcementRuleDefinedEvent** — Rule created with name, code, category, scope, severity
- **EnforcementRuleActivatedEvent** — Rule activated
- **EnforcementRuleDisabledEvent** — Rule disabled
- **EnforcementRuleRetiredEvent** — Rule retired (terminal)
- **ViolationDetectedEvent** — Rule breach detected with rule reference, source, and reason
- **ViolationAcknowledgedEvent** — Violation acknowledged
- **ViolationResolvedEvent** — Violation resolved with resolution description

## Specifications
- **IsActiveSpecification** (rule) — Status == Active
- **CanAcknowledgeSpecification** (violation) — Status == Open
- **CanResolveSpecification** (violation) — Status == Acknowledged

## Domain Services
- **EnforcementRuleService** (rule) — Stub implementation ready for domain logic
- **ViolationService** (violation) — Stub implementation ready for domain logic
- **ViolationTraceService** (violation) — Validates rule reference and source reference are non-empty

## Invariants (CRITICAL)
- Every violation must reference a rule
- Every violation must reference a source
- Rules must be uniquely identifiable (no duplicate codes)
- Violations must not exist without a triggering condition (reason required)
- Enforcement does not mutate financial truth

## Policy Dependencies
- Non-mutative constraint — enforcement never alters financial state
- Rule uniqueness enforcement (no duplicate rule codes)
- Violation traceability enforcement (must reference rule and source)

## Integration Points
- **rule -> capital / ledger / transaction / revenue** — Rules scope to economic contexts
- **violation -> rule** — Violations reference breached rules
- **violation -> source domain** — Violations reference triggering aggregate or event
- **compliance** — Enforcement violations may trigger audit evidence capture

## Lifecycle

### Rule
```
Define() -> Active
  Disable() -> Disabled
  Activate() -> Active (from Disabled)
  Retire() -> Retired (terminal)
```

### Violation
```
Detect() -> Open (requires rule reference, source, reason)
  Acknowledge() -> Acknowledged
  Resolve() -> Resolved (terminal, requires resolution description)
```

## Notes
- Pure domain — zero runtime, infrastructure, or engine dependencies
- EnforcementRuleErrors and ViolationErrors use factory methods returning exceptions
- Both service stubs are ready for domain logic implementation
