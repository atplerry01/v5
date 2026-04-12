# Domain: Rule

## Classification
economic-system

## Context
enforcement

## Purpose
Defines enforceable economic rules that apply across economic contexts (capital, ledger, transaction, revenue, exposure, reconciliation). Rules are evaluated against subjects and produce pass/fail outcomes. Failed evaluations trigger violations in the violation domain.

## Core Responsibilities
- Defining economic rules with name, description, code, category, scope, and severity
- Managing rule lifecycle (Active -> Disabled -> Retired)
- Activating and disabling rules as needed
- Retiring rules permanently

## Aggregate(s)
- **EnforcementRuleAggregate**
  - Event-sourced, sealed. Manages rule definition and lifecycle
  - Invariants: Must have non-empty name; cannot retire a Retired rule; cannot disable a non-Active rule; cannot activate a Retired rule

## Entities
None

## Value Objects
- **RuleId** — Typed Guid wrapper with From() factory for unique rule identity
- **RuleCode** — Machine-readable rule code (string, cannot be empty/whitespace)
- **RuleCategory** — Rule categorization for grouping (string, cannot be empty/whitespace)
- **RuleScope** — Enum: Capital, Ledger, Transaction, Revenue, Exposure, Reconciliation
- **RuleSeverity** — Enum: Low, Medium, High, Critical
- **RuleStatus** — Enum: Active, Disabled, Retired

## Domain Events
- **EnforcementRuleDefinedEvent** — Rule created (captures RuleId, RuleCode, RuleName, RuleCategory, Scope, Severity, Description)
- **EnforcementRuleActivatedEvent** — Rule activated
- **EnforcementRuleDisabledEvent** — Rule disabled
- **EnforcementRuleRetiredEvent** — Rule retired (terminal)

## Specifications
- **IsActiveSpecification** — Status == Active

## Domain Services
- **EnforcementRuleService** — Stub implementation ready for domain logic

## Invariants (CRITICAL)
- Rules must have a non-empty name
- Only Active rules can be disabled
- Retired rules cannot be activated or disabled
- Rules must be uniquely identifiable (no duplicate codes)

## Policy Dependencies
- Rule uniqueness enforcement (no duplicate rule codes)
- Scope validity enforcement

## Integration Points
- **violation** — Failed rule evaluations trigger violations in the violation domain
- **capital / ledger / transaction / revenue** — Rules scope to these economic contexts

## Lifecycle
```
Define() -> Active
  Disable() -> Disabled
  Activate() -> Active (reactivation from Disabled)
  Retire() -> Retired (terminal, cannot transition further)
```

## Notes
- EnforcementRuleErrors uses factory methods returning exceptions
- EnforcementRuleService is a stub ready for domain logic
- RuleCode and RuleCategory use string-based value objects with validation
