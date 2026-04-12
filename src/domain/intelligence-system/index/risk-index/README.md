# Domain: RiskIndex

## Classification

intelligence-system

## Context

index

## Purpose

Defines the structure of risk index records — the composite indicators measuring aggregate risk levels.

## Core Responsibilities

* Define the canonical structure for risk index records
* Track lifecycle state of risk index entries
* Emit domain events on creation, update, and state transitions

## Aggregate(s)

* RiskIndexAggregate

  * Represents a single risk index record with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* RiskIndexId — Unique identifier for a risk-index instance

## Domain Events

* RiskIndexCreatedEvent — Raised when a new risk-index is created
* RiskIndexUpdatedEvent — Raised when risk-index metadata is updated
* RiskIndexStateChangedEvent — Raised when risk-index lifecycle state transitions

## Specifications

* RiskIndexSpecification — Validates risk-index structure and completeness

## Domain Services

* RiskIndexService — Domain operations for risk-index management

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
