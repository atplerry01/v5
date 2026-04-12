# Domain: Analysis

## Classification

intelligence-system

## Context

economic

## Purpose

Defines the structure of economic analysis artifacts — the structured outputs of economic assessments.

## Core Responsibilities

* Define analysis structure and metadata
* Track lifecycle state of analysis artifacts
* Enforce structural invariants for analysis validity

## Aggregate(s)

* AnalysisAggregate

  * Manages the lifecycle and invariants of an analysis artifact

## Entities

* None

## Value Objects

* AnalysisId — Unique identifier for an analysis instance

## Domain Events

* AnalysisCreatedEvent — Raised when a new analysis is created
* AnalysisUpdatedEvent — Raised when analysis metadata is updated
* AnalysisStateChangedEvent — Raised when analysis lifecycle state transitions

## Specifications

* AnalysisSpecification — Validates analysis structure and completeness

## Domain Services

* AnalysisService — Domain operations for analysis management

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
