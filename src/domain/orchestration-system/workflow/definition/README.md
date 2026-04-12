# Domain: Definition

## Classification

orchestration-system

## Context

workflow

## Purpose

This domain defines workflow blueprint structure only and contains no workflow execution or scheduling logic.

## Core Responsibilities

* Define workflow blueprint structure (name and version)
* Manage definition lifecycle: Draft, Published, Retired (terminal)
* Enforce valid state transitions via specifications

## Aggregate(s)

* DefinitionAggregate

  * Factory: `Draft(id, blueprint)` creates a new draft definition
  * Transitions: `Publish()`, `Retire()`
  * Lifecycle is terminal: Draft -> Published -> Retired

## Entities

* None

## Value Objects

* DefinitionId — Validated Guid identifier (rejects Guid.Empty)
* DefinitionStatus — Enum: Draft, Published, Retired
* WorkflowBlueprint — Record struct with WorkflowName (non-empty string) and Version (> 0)

## Domain Events

* DefinitionDraftedEvent(DefinitionId, WorkflowBlueprint) — Raised when a new definition is drafted
* DefinitionPublishedEvent(DefinitionId) — Raised when a draft definition is published
* DefinitionRetiredEvent(DefinitionId) — Raised when a published definition is retired

## Specifications

* CanPublishSpecification — Satisfied when status is Draft
* CanRetireSpecification — Satisfied when status is Published

## Errors

* MissingId — Definition identifier is required
* MissingBlueprint — Valid workflow blueprint is required
* InvalidStateTransition(status, action) — InvalidOperationException for illegal transitions

## Domain Services

* DefinitionService — Empty (no domain service operations required)

## Invariants

* Definition identifier must not be Guid.Empty
* Blueprint must have a non-empty name and version > 0
* State transitions must follow terminal lifecycle: Draft -> Published -> Retired
* No execution logic inside domain

## Policy Dependencies

* Workflow constraints may be governed by WHYCEPOLICY

## Integration Points

* decision-system
* operational-system
* runtime (external execution only)

## Lifecycle

Draft -> Published -> Retired (TERMINAL)

## Notes

This domain defines orchestration structure ONLY. Execution is handled externally by engines/runtime.
