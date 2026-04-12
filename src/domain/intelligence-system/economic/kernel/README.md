# Domain: Kernel

## Classification

intelligence-system

## Context

economic

## Purpose

Defines the structure of economic kernel records — the core computational parameters for economic models.

## Core Responsibilities

* Define kernel structure and metadata
* Track lifecycle state of kernel records
* Enforce structural invariants for kernel validity

## Aggregate(s)

* KernelAggregate

  * Manages the lifecycle and invariants of a kernel record

## Entities

* None

## Value Objects

* KernelId — Unique identifier for a kernel instance

## Domain Events

* KernelCreatedEvent — Raised when a new kernel is created
* KernelUpdatedEvent — Raised when kernel metadata is updated
* KernelStateChangedEvent — Raised when kernel lifecycle state transitions

## Specifications

* KernelSpecification — Validates kernel structure and completeness

## Domain Services

* KernelService — Domain operations for kernel management

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
