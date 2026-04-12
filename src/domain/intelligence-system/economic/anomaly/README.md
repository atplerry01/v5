# Domain: Anomaly

## Classification

intelligence-system

## Context

economic

## Purpose

Defines the structure of economic anomaly records — detected deviations from expected economic patterns.

## Core Responsibilities

* Define anomaly structure and metadata
* Track lifecycle state of anomaly records
* Enforce structural invariants for anomaly validity

## Aggregate(s)

* AnomalyAggregate

  * Manages the lifecycle and invariants of an anomaly record

## Entities

* None

## Value Objects

* AnomalyId — Unique identifier for an anomaly instance

## Domain Events

* AnomalyCreatedEvent — Raised when a new anomaly is created
* AnomalyUpdatedEvent — Raised when anomaly metadata is updated
* AnomalyStateChangedEvent — Raised when anomaly lifecycle state transitions

## Specifications

* AnomalySpecification — Validates anomaly structure and completeness

## Domain Services

* AnomalyService — Domain operations for anomaly management

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
