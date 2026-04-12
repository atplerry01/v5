# Domain: Benchmark

## Classification

business-system

## Context

portfolio

## Purpose

Defines the structure of portfolio benchmarks — the immutable reference standards against which portfolio performance is compared.

## Boundary Statement

This domain defines portfolio behavior contracts only and contains no execution logic.

## Core Responsibilities

* Define and maintain benchmark reference structure and metadata
* Track benchmark lifecycle state transitions
* Enforce structural invariants for benchmark consistency
* Must be immutable once active — benchmarks define comparison baselines, not calculations

## Aggregate(s)

* BenchmarkAggregate
  * Represents the root entity for a portfolio benchmark, encapsulating its structure and lifecycle

## Value Objects

* BenchmarkId — Unique identifier for a benchmark instance
* BenchmarkName — Descriptive name for the benchmark reference
* BenchmarkStatus — Lifecycle state (Draft, Active, Retired)

## Domain Events

* BenchmarkCreatedEvent — Raised when a new benchmark is created
* BenchmarkActivatedEvent — Raised when a benchmark is activated as a reference standard
* BenchmarkRetiredEvent — Raised when a benchmark is retired from use
* BenchmarkUpdatedEvent — Raised when benchmark metadata is updated
* BenchmarkStateChangedEvent — Raised when benchmark lifecycle state transitions

## Specifications

* BenchmarkSpecification — Validates benchmark structure and completeness
* CanActivateSpecification — Draft → Active transition guard
* CanRetireSpecification — Active → Retired transition guard

## Domain Services

* BenchmarkService — Reserved for cross-aggregate coordination within benchmark context

## Invariants

* BenchmarkId must not be empty
* BenchmarkName must not be empty
* Status must be a defined enum value
* Must define comparison reference, must be immutable once active
* No financial or execution logic allowed

## Lifecycle Pattern

TERMINAL: Draft → Active → Retired

* Draft: Initial state upon creation
* Active: Benchmark is the active reference standard
* Retired: Terminal state, benchmark is no longer in use

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed.
