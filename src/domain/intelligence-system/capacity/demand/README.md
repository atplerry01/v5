# Domain: Demand

## Classification

intelligence-system

## Context

capacity

## Purpose

Defines the structure of capacity demand records — the anticipated or measured need for resources.

## Core Responsibilities

* Define demand structure and metadata
* Track lifecycle state of demand records
* Enforce structural invariants for demand validity

## Aggregate(s)

* DemandAggregate

  * Manages the lifecycle and invariants of a demand record

## Entities

* None

## Value Objects

* DemandId — Unique identifier for a demand instance

## Domain Events

* DemandCreatedEvent — Raised when a new demand is created
* DemandUpdatedEvent — Raised when demand metadata is updated
* DemandStateChangedEvent — Raised when demand lifecycle state transitions

## Specifications

* DemandSpecification — Validates demand structure and completeness

## Domain Services

* DemandService — Domain operations for demand management

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
