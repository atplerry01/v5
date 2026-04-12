# Domain: Metric

## Classification

intelligence-system

## Context

observability

## Purpose

Defines the structure of metric records — the quantitative measurements of system behaviour and performance.

## Core Responsibilities

* Define the canonical structure of metric artifacts
* Enforce metric validity and completeness invariants
* Emit domain events on metric lifecycle transitions

## Aggregate(s)

* MetricAggregate

  * Root aggregate representing a metric record instance and its lifecycle

## Entities

* None

## Value Objects

* MetricId — Unique identifier for a metric instance

## Domain Events

* MetricCreatedEvent — Raised when a new metric is created
* MetricUpdatedEvent — Raised when metric metadata is updated
* MetricStateChangedEvent — Raised when metric lifecycle state transitions

## Specifications

* MetricSpecification — Validates metric structure and completeness

## Domain Services

* MetricService — Domain operations for metric management

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
