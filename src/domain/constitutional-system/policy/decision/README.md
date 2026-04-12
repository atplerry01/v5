# Domain: Decision

## Classification

constitutional-system

## Context

policy

## Purpose

Defines the constitutional policy decision event structure — the record of policy evaluation outcomes. This domain captures the shape of decisions (allow/deny) as domain events, representing what a policy decision looks like structurally. Decisions themselves are made externally by WHYCEPOLICY.

## Core Responsibilities

* Define policy decision event structure
* Define decision outcome recording shape (allow, deny)
* Define decision traceability references (correlation, causation, hash)

## Aggregate(s)

* None — decisions are external outcomes, not internally managed state

## Entities

* None

## Value Objects

* None

## Domain Events

* PolicyEvaluatedEvent — Records an ALLOW policy decision outcome
* PolicyDeniedEvent — Records a DENY policy decision outcome

## Specifications

* None

## Domain Services

* None

## Invariants

* Decision events must capture the policy engine's hash, never regenerate it
* Decision events must reference upstream correlation and causation IDs
* Decision events must not use Guid.NewGuid, IClock, or DateTime.UtcNow
* All fields must be sourced from upstream context

## Policy Dependencies

* WHYCEPOLICY is the execution authority
* Domain only defines event structure, not decision logic

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
This domain is intentionally event-only — decisions are made by the policy engine, and this domain defines the canonical shape of those decision records.
