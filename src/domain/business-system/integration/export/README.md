# Domain: Export

## Classification

business-system

## Context

integration

## Purpose

Defines the structure of integration exports — the outbound data extraction records for sending data to external systems.

## Core Responsibilities

* Define the structural representation of integration exports
* Track export metadata and lifecycle state
* Emit domain events on export state changes

## Aggregate(s)

* ExportAggregate

  * Represents the root entity for an integration export, encapsulating its identity, metadata, and lifecycle state

## Entities

* None

## Value Objects

* ExportId — Unique identifier for an export instance

## Domain Events

* ExportCreatedEvent — Raised when a new export is created
* ExportUpdatedEvent — Raised when export metadata is updated
* ExportStateChangedEvent — Raised when export lifecycle state transitions

## Specifications

* ExportSpecification — Validates export structure and completeness

## Domain Services

* ExportService — Domain operations for export management

## Invariants

* Business entities must remain consistent
* Relationships must be valid
* No financial or execution logic allowed

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Lifecycle

Created → Active → Updated → Inactive

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed.
