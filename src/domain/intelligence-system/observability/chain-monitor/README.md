# Domain: ChainMonitor

## Classification

intelligence-system

## Context

observability

## Purpose

Defines the structure of chain monitor records — the surveillance artifacts tracking chain integrity and health.

## Core Responsibilities

* Define the canonical structure of chain monitor artifacts
* Enforce chain monitor validity and completeness invariants
* Emit domain events on chain monitor lifecycle transitions

## Aggregate(s)

* ChainMonitorAggregate

  * Root aggregate representing a chain monitor instance and its lifecycle

## Entities

* None

## Value Objects

* ChainMonitorId — Unique identifier for a chain monitor instance

## Domain Events

* ChainMonitorCreatedEvent — Raised when a new chain monitor is created
* ChainMonitorUpdatedEvent — Raised when chain monitor metadata is updated
* ChainMonitorStateChangedEvent — Raised when chain monitor lifecycle state transitions

## Specifications

* ChainMonitorSpecification — Validates chain monitor structure and completeness

## Domain Services

* ChainMonitorService — Domain operations for chain monitor management

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
