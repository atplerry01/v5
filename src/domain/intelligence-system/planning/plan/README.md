# Domain: Plan

## Classification

intelligence-system

## Context

planning

## Purpose

Defines the structure of plans — the root planning entity containing strategies and resource allocations.

## Core Responsibilities

* Define the canonical structure of plan artifacts
* Enforce plan validity and completeness invariants
* Emit domain events on plan lifecycle transitions

## Aggregate(s)

* PlanAggregate

  * Root aggregate representing a plan instance and its lifecycle

## Entities

* None

## Value Objects

* PlanId — Unique identifier for a plan instance

## Domain Events

* PlanCreatedEvent — Raised when a new plan is created
* PlanUpdatedEvent — Raised when plan metadata is updated
* PlanStateChangedEvent — Raised when plan lifecycle state transitions

## Specifications

* PlanSpecification — Validates plan structure and completeness

## Domain Services

* PlanService — Domain operations for plan management

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
