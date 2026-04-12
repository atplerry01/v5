# Domain: Supply

## Classification

intelligence-system

## Context

capacity

## Purpose

Defines the structure of capacity supply records — the available resources and their quantities.

## Core Responsibilities

* Define supply structure and metadata
* Track lifecycle state of supply records
* Enforce structural invariants for supply validity

## Aggregate(s)

* SupplyAggregate

  * Manages the lifecycle and invariants of a supply record

## Entities

* None

## Value Objects

* SupplyId — Unique identifier for a supply instance

## Domain Events

* SupplyCreatedEvent — Raised when a new supply is created
* SupplyUpdatedEvent — Raised when supply metadata is updated
* SupplyStateChangedEvent — Raised when supply lifecycle state transitions

## Specifications

* SupplySpecification — Validates supply structure and completeness

## Domain Services

* SupplyService — Domain operations for supply management

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
