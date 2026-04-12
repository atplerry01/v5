# Domain: Hypothesis

## Classification

intelligence-system

## Context

experiment

## Purpose

Defines the structure of experiment hypotheses — the testable propositions that experiments aim to validate or refute.

## Core Responsibilities

* Define hypothesis structure and metadata
* Enforce hypothesis invariants and validation rules
* Emit domain events on hypothesis lifecycle transitions

## Aggregate(s)

* HypothesisAggregate

  * Root aggregate representing an experiment hypothesis with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* HypothesisId — Unique identifier for a hypothesis instance

## Domain Events

* HypothesisCreatedEvent — Raised when a new hypothesis is created
* HypothesisUpdatedEvent — Raised when hypothesis metadata is updated
* HypothesisStateChangedEvent — Raised when hypothesis lifecycle state transitions

## Specifications

* HypothesisSpecification — Validates hypothesis structure and completeness

## Domain Services

* HypothesisService — Domain operations for hypothesis management

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
