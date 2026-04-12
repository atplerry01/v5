# Domain: Log

## Classification

intelligence-system

## Context

observability

## Purpose

Defines the structure of log records — the structured entries capturing system events and activities.

## Core Responsibilities

* Define the canonical structure of log artifacts
* Enforce log record validity and completeness invariants
* Emit domain events on log lifecycle transitions

## Aggregate(s)

* LogAggregate

  * Root aggregate representing a log record instance and its lifecycle

## Entities

* None

## Value Objects

* LogId — Unique identifier for a log instance

## Domain Events

* LogCreatedEvent — Raised when a new log is created
* LogUpdatedEvent — Raised when log metadata is updated
* LogStateChangedEvent — Raised when log lifecycle state transitions

## Specifications

* LogSpecification — Validates log structure and completeness

## Domain Services

* LogService — Domain operations for log management

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
