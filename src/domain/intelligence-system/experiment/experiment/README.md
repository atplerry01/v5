# Domain: Experiment

## Classification

intelligence-system

## Context

experiment

## Purpose

Defines the structure of experiments — the root entity representing a controlled test with defined hypotheses and variants.

## Core Responsibilities

* Define experiment structure and metadata
* Enforce experiment invariants and validation rules
* Emit domain events on experiment lifecycle transitions

## Aggregate(s)

* ExperimentAggregate

  * Root aggregate representing an experiment with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* ExperimentId — Unique identifier for an experiment instance

## Domain Events

* ExperimentCreatedEvent — Raised when a new experiment is created
* ExperimentUpdatedEvent — Raised when experiment metadata is updated
* ExperimentStateChangedEvent — Raised when experiment lifecycle state transitions

## Specifications

* ExperimentSpecification — Validates experiment structure and completeness

## Domain Services

* ExperimentService — Domain operations for experiment management

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
