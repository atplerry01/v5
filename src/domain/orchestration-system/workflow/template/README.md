# Domain: Template

## Classification

orchestration-system

## Context

workflow

## Purpose

Defines the structure of workflow templates — reusable workflow patterns that can be instantiated into concrete workflow definitions. Represents template composition and versioning.

## Core Responsibilities

* Define template structure and composition
* Track template versioning
* Maintain template lifecycle state

## Aggregate(s)

* TemplateAggregate

  * Represents workflow template container

## Entities

* None

## Value Objects

* TemplateId — Unique identifier for a template instance

## Domain Events

* TemplateCreatedEvent — Raised when a new template is created
* TemplateUpdatedEvent — Raised when template metadata is updated
* TemplateStateChangedEvent — Raised when template lifecycle state transitions

## Specifications

* TemplateSpecification — Validates template structure and completeness

## Domain Services

* TemplateService — Domain operations for template management

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
