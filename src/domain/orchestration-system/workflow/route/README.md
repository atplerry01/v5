# Domain: Route

## Classification

orchestration-system

## Context

workflow

## Purpose

Defines the structure of workflow routes — the pathways through which workflow execution flows between stages and steps. Represents routing topology, not runtime dispatch.

## Core Responsibilities

* Define route structure and topology
* Track route targets and conditions
* Maintain route state within workflow definition

## Aggregate(s)

* RouteAggregate

  * Represents workflow route container

## Entities

* None

## Value Objects

* RouteId — Unique identifier for a route instance

## Domain Events

* RouteCreatedEvent — Raised when a new route is created
* RouteUpdatedEvent — Raised when route metadata is updated
* RouteStateChangedEvent — Raised when route lifecycle state transitions

## Specifications

* RouteSpecification — Validates route structure and completeness

## Domain Services

* RouteService — Domain operations for route management

## Invariants

* Orchestration must be deterministic
* State transitions must be valid
* No execution logic inside domain

## Policy Dependencies

* Workflow constraints may be governed by WHYCEPOLICY

## Integration Points

* decision-system
* operational-system
* runtime (external execution only)

## Lifecycle

Defined → Started → In-Progress → Completed → Failed → Terminated

## Notes

This domain defines orchestration structure ONLY. Execution is handled externally by engines/runtime.
