# Domain: DemandSupply

## Classification

intelligence-system

## Context

estimation

## Purpose

Defines the structure of demand-supply estimation records — the projected balance between demand and supply.

## Core Responsibilities

* Define demand-supply structure and metadata
* Enforce demand-supply invariants and validation rules
* Emit domain events on demand-supply lifecycle transitions

## Aggregate(s)

* DemandSupplyAggregate

  * Root aggregate representing a demand-supply record with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* DemandSupplyId — Unique identifier for a demand-supply instance

## Domain Events

* DemandSupplyCreatedEvent — Raised when a new demand-supply is created
* DemandSupplyUpdatedEvent — Raised when demand-supply metadata is updated
* DemandSupplyStateChangedEvent — Raised when demand-supply lifecycle state transitions

## Specifications

* DemandSupplySpecification — Validates demand-supply structure and completeness

## Domain Services

* DemandSupplyService — Domain operations for demand-supply management

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
