# Domain: Session

## Classification

trust-system

## Context

access

## Purpose

Represents the domain responsible for managing access sessions — time-bounded, stateful contexts in which a principal exercises their access rights after successful authentication.

## Core Responsibilities

* Create and manage session lifecycle within trust boundaries
* Enforce session validity, duration, and renewal rules
* Emit events when sessions are created, refreshed, or terminated

## Aggregate(s)

* SessionAggregate
  * Enforces invariants around session creation, duration, and termination
  * Validates session state before committing changes

## Entities

* None

## Value Objects

* SessionId — Strongly-typed identifier for a session

## Domain Events

* SessionCreatedEvent — Raised when a new session is established
* SessionStateChangedEvent — Raised when session state transitions (e.g., active → expired)
* SessionUpdatedEvent — Raised when an existing session is refreshed or modified

## Specifications

* SessionSpecification — Validates session eligibility and renewal criteria

## Domain Services

* SessionService — Coordinates session lifecycle management logic

## Invariants

* A session must reference a valid authenticated principal
* Sessions must not exceed maximum duration or idle timeout
* State transitions must pass pre-change validation

## Policy Dependencies

* Session duration limits, idle timeouts, concurrent session limits, renewal policies (WHYCEPOLICY controlled)

## Integration Points

* Identity (WhyceID) — Session principal resolution
* Authorization — Active session required for authorization evaluation
* Device (identity context) — Session may be bound to a specific device
* Governance — Session activity audit trail

## Lifecycle

Created → Active → Refreshed → Expired | Terminated. All transitions emit domain events and enforce invariants.

## Notes

Sessions represent the runtime access context. They are distinct from authentication (which establishes identity) and authorization (which evaluates permissions). Session policies including concurrent session limits and forced termination are WHYCEPOLICY controlled.
