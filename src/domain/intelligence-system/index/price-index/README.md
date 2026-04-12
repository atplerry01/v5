# Domain: PriceIndex

## Classification

intelligence-system

## Context

index

## Purpose

Defines the structure of price index records — the composite indicators tracking price movements across markets.

## Core Responsibilities

* Define the canonical structure for price index records
* Track lifecycle state of price index entries
* Emit domain events on creation, update, and state transitions

## Aggregate(s)

* PriceIndexAggregate

  * Represents a single price index record with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* PriceIndexId — Unique identifier for a price-index instance

## Domain Events

* PriceIndexCreatedEvent — Raised when a new price-index is created
* PriceIndexUpdatedEvent — Raised when price-index metadata is updated
* PriceIndexStateChangedEvent — Raised when price-index lifecycle state transitions

## Specifications

* PriceIndexSpecification — Validates price-index structure and completeness

## Domain Services

* PriceIndexService — Domain operations for price-index management

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
