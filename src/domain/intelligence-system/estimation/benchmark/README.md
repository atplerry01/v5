# Domain: Benchmark

## Classification

intelligence-system

## Context

estimation

## Purpose

Defines the structure of estimation benchmarks — reference data points used to calibrate estimates.

## Core Responsibilities

* Define benchmark structure and metadata
* Enforce benchmark invariants and validation rules
* Emit domain events on benchmark lifecycle transitions

## Aggregate(s)

* BenchmarkAggregate

  * Root aggregate representing an estimation benchmark with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* BenchmarkId — Unique identifier for a benchmark instance

## Domain Events

* BenchmarkCreatedEvent — Raised when a new benchmark is created
* BenchmarkUpdatedEvent — Raised when benchmark metadata is updated
* BenchmarkStateChangedEvent — Raised when benchmark lifecycle state transitions

## Specifications

* BenchmarkSpecification — Validates benchmark structure and completeness

## Domain Services

* BenchmarkService — Domain operations for benchmark management

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
