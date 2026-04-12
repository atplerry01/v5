# Domain: Objective

## Classification

intelligence-system

## Context

planning

## Purpose

Defines the structure of planning objectives — the goals and targets that plans aim to achieve.

## Core Responsibilities

* Define the canonical structure of objective artifacts
* Enforce objective validity and completeness invariants
* Emit domain events on objective lifecycle transitions

## Aggregate(s)

* ObjectiveAggregate

  * Root aggregate representing an objective instance and its lifecycle

## Entities

* None

## Value Objects

* ObjectiveId — Unique identifier for an objective instance

## Domain Events

* ObjectiveCreatedEvent — Raised when a new objective is created
* ObjectiveUpdatedEvent — Raised when objective metadata is updated
* ObjectiveStateChangedEvent — Raised when objective lifecycle state transitions

## Specifications

* ObjectiveSpecification — Validates objective structure and completeness

## Domain Services

* ObjectiveService — Domain operations for objective management

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
