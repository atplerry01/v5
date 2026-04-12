# Domain: Autonomy

## Classification

intelligence-system

## Context

economic

## Purpose

Defines the structure of economic autonomy records — the degree of self-governance in economic decisions.

## Core Responsibilities

* Define autonomy structure and metadata
* Track lifecycle state of autonomy records
* Enforce structural invariants for autonomy validity

## Aggregate(s)

* AutonomyAggregate

  * Manages the lifecycle and invariants of an autonomy record

## Entities

* None

## Value Objects

* AutonomyId — Unique identifier for an autonomy instance

## Domain Events

* AutonomyCreatedEvent — Raised when a new autonomy is created
* AutonomyUpdatedEvent — Raised when autonomy metadata is updated
* AutonomyStateChangedEvent — Raised when autonomy lifecycle state transitions

## Specifications

* AutonomySpecification — Validates autonomy structure and completeness

## Domain Services

* AutonomyService — Domain operations for autonomy management

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
