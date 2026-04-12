# Domain: Variant

## Classification

intelligence-system

## Context

experiment

## Purpose

Defines the structure of experiment variants — the different treatments or configurations being compared in an experiment.

## Core Responsibilities

* Define variant structure and metadata
* Enforce variant invariants and validation rules
* Emit domain events on variant lifecycle transitions

## Aggregate(s)

* VariantAggregate

  * Root aggregate representing an experiment variant with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* VariantId — Unique identifier for a variant instance

## Domain Events

* VariantCreatedEvent — Raised when a new variant is created
* VariantUpdatedEvent — Raised when variant metadata is updated
* VariantStateChangedEvent — Raised when variant lifecycle state transitions

## Specifications

* VariantSpecification — Validates variant structure and completeness

## Domain Services

* VariantService — Domain operations for variant management

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
