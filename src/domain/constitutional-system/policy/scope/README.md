# Domain: Scope

## Classification

constitutional-system

## Context

policy

## Purpose

Defines constitutional scope boundaries — the extent and applicability of policies within the system. Scope determines which parts of the system a policy applies to, including classification, context, and domain targeting.

## Core Responsibilities

* Define scope structure and identity
* Define scope boundaries and applicability rules
* Define governance authority references for scope resolution

## Aggregate(s)

* ScopeAggregate

  * Represents the policy scope container

## Entities

* None

## Value Objects

* ScopeId — Unique identifier for a scope definition

## Domain Events

* ScopeCreatedEvent — Raised when a new scope is defined
* ScopeUpdatedEvent — Raised when scope metadata is updated
* ScopeStateChangedEvent — Raised when scope lifecycle state transitions

## Specifications

* ScopeSpecification — Validates scope structure and completeness

## Domain Services

* ScopeService — Domain operations for scope management

## Invariants

* Scopes must be immutable once activated (unless versioned)
* Scopes must be traceable
* Scopes must be policy-bound (WHYCEPOLICY)

## Policy Dependencies

* WHYCEPOLICY is the execution authority
* Domain only defines structure, not enforcement

## Integration Points

* decision-system
* trust-system
* economic-system
* WHYCEPOLICY engine (external)

## Lifecycle

Draft → Defined → Activated → Versioned → Deprecated

## Notes

This domain defines constitutional structure ONLY.
All enforcement is external via WHYCEPOLICY and runtime.
