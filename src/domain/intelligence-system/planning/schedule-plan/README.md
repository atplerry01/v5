# Domain: SchedulePlan

## Classification

intelligence-system

## Context

planning

## Purpose

Defines the structure of schedule plans — the time-based planning artifacts organizing activities across periods.

## Core Responsibilities

* Define the canonical structure of schedule plan artifacts
* Enforce schedule plan validity and completeness invariants
* Emit domain events on schedule plan lifecycle transitions

## Aggregate(s)

* SchedulePlanAggregate

  * Root aggregate representing a schedule plan instance and its lifecycle

## Entities

* None

## Value Objects

* SchedulePlanId — Unique identifier for a schedule plan instance

## Domain Events

* SchedulePlanCreatedEvent — Raised when a new schedule plan is created
* SchedulePlanUpdatedEvent — Raised when schedule plan metadata is updated
* SchedulePlanStateChangedEvent — Raised when schedule plan lifecycle state transitions

## Specifications

* SchedulePlanSpecification — Validates schedule plan structure and completeness

## Domain Services

* SchedulePlanService — Domain operations for schedule plan management

## Invariants

* Intelligence artifacts must be deterministic and traceable
* No execution logic allowed
* No inference logic allowed

## Policy Dependencies

* Governance or usage constraints may be governed by WHYCEPOLICY

## Integration Points

* decision-system (consumes insights)
* trust-system (signals influence trust)
* economic-system (signals influence risk)

## Lifecycle

Created → Updated → Evaluated → Archived

## Notes

This domain represents intelligence structure ONLY. All AI/ML execution is external (T3I layer).
