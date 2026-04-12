# Domain: IdentityGraph

## Classification

trust-system

## Context

identity

## Purpose

Represents the domain responsible for managing the identity graph — the relational topology that maps connections, trust paths, and linkages between identities within and across trust boundaries.

## Core Responsibilities

* Build and maintain the graph of identity relationships
* Track connection types, strengths, and trust paths
* Emit events when graph edges are created, modified, or removed

## Aggregate(s)

* IdentityGraphAggregate
  * Enforces invariants around graph edge creation and topology constraints
  * Validates graph integrity before committing changes

## Entities

* None

## Value Objects

* IdentityGraphId — Strongly-typed identifier for an identity graph record

## Domain Events

* IdentityGraphCreatedEvent — Raised when a new graph node or edge is established
* IdentityGraphStateChangedEvent — Raised when graph state transitions
* IdentityGraphUpdatedEvent — Raised when graph topology is modified

## Specifications

* IdentityGraphSpecification — Validates graph topology and connection criteria

## Domain Services

* IdentityGraphService — Coordinates graph construction and query logic

## Invariants

* Graph edges must reference valid, active identities
* Circular trust paths must be detected and prevented where policy requires
* Graph modifications must not violate maximum connection depth

## Policy Dependencies

* Maximum graph depth, connection limits, trust path scoring rules (WHYCEPOLICY controlled)

## Integration Points

* Identity (WhyceID) — Graph nodes are identities
* Federation — Federated links contribute graph edges
* Trust — Graph topology influences trust scoring
* Governance — Graph audit trail

## Lifecycle

Node/Edge Created → Active → Updated → Removed. All transitions emit domain events and enforce invariants.

## Notes

The identity graph provides a structural view of how identities relate to each other. It supports trust path analysis, influence mapping, and federation topology. Graph constraints and scoring are WHYCEPOLICY controlled.
