# Domain: ResultAnalysis

## Classification

intelligence-system

## Context

experiment

## Purpose

Defines the structure of experiment result analysis records — the structured outcomes and statistical summaries of completed experiments.

## Core Responsibilities

* Define result analysis structure and metadata
* Enforce result analysis invariants and validation rules
* Emit domain events on result analysis lifecycle transitions

## Aggregate(s)

* ResultAnalysisAggregate

  * Root aggregate representing an experiment result analysis with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* ResultAnalysisId — Unique identifier for a result analysis instance

## Domain Events

* ResultAnalysisCreatedEvent — Raised when a new result analysis is created
* ResultAnalysisUpdatedEvent — Raised when result analysis metadata is updated
* ResultAnalysisStateChangedEvent — Raised when result analysis lifecycle state transitions

## Specifications

* ResultAnalysisSpecification — Validates result analysis structure and completeness

## Domain Services

* ResultAnalysisService — Domain operations for result analysis management

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
