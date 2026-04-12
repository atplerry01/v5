# Domain: Rule

## Classification

constitutional-system

## Context

policy

## Purpose

Defines the policy rule structure — the representation of policy rules within the constitutional system. Rules represent individual policy declarations that express what is required, permitted, or prohibited.

## Boundary

This domain defines policy rule structure only and contains no policy evaluation or enforcement logic.

## Core Responsibilities

* Define rule structure and identity
* Define rule lifecycle (Draft, Active, Retired)
* Enforce structural invariants on rule definitions

## Aggregate(s)

* RuleAggregate

  * Factory: Draft(id, definition)
  * Transitions: Activate(), Retire()
  * Lifecycle: TERMINAL (Draft -> Active -> Retired)

## Entities

* None

## Value Objects

* RuleId — Validated unique identifier (Guid, non-empty)
* RuleStatus — Enum: Draft, Active, Retired
* RuleDefinition — Record struct: RuleName (string, non-empty), PolicyReference (string, non-empty)

## Domain Events

* RuleDraftedEvent — Raised when a new rule is drafted
* RuleActivatedEvent — Raised when a rule is activated
* RuleRetiredEvent — Raised when a rule is retired

## Specifications

* CanActivateSpecification — Status must be Draft
* CanRetireSpecification — Status must be Active

## Errors

* MissingId — RuleId is required
* MissingDefinition — Rule must include a valid definition
* InvalidStateTransition(status, action) — Invalid lifecycle transition

## Domain Services

* RuleService — Empty (no domain operations required)

## Invariants

* RuleId must not be empty
* RuleDefinition must not be default
* RuleStatus must be a defined enum value
* Lifecycle is terminal: Draft -> Active -> Retired (no reverse transitions)

## Policy Dependencies

* WHYCEPOLICY is the execution authority
* Domain only defines structure, not enforcement

## Integration Points

* None (structure-only domain)

## Lifecycle

Draft -> Active -> Retired (TERMINAL)
