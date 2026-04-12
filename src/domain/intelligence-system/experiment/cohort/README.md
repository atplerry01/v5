# Domain: Cohort

## Classification

intelligence-system

## Context

experiment

## Purpose

Defines the structure of experiment cohorts — the groups of subjects assigned to experiment variants.

## Core Responsibilities

* Define cohort structure and metadata
* Enforce cohort invariants and validation rules
* Emit domain events on cohort lifecycle transitions

## Aggregate(s)

* CohortAggregate

  * Root aggregate representing an experiment cohort with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* CohortId — Unique identifier for a cohort instance

## Domain Events

* CohortCreatedEvent — Raised when a new cohort is created
* CohortUpdatedEvent — Raised when cohort metadata is updated
* CohortStateChangedEvent — Raised when cohort lifecycle state transitions

## Specifications

* CohortSpecification — Validates cohort structure and completeness

## Domain Services

* CohortService — Domain operations for cohort management

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
