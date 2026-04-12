# Domain: CommandCatalog

## Classification

core-system

## Context

command

## Purpose

Defines the catalog of all available commands in the system. Provides the registry of command types and their metadata for cross-system command discovery.

## Core Responsibilities

* Maintaining a registry of all known command types in the system
* Providing command metadata for discovery and introspection
* Tracking command type availability and lifecycle status

## Aggregate(s)

* CommandCatalogAggregate

  * Manages the lifecycle and state of a command catalog entry

## Entities

* None

## Value Objects

* CommandCatalogId — Unique identifier for a command-catalog instance

## Domain Events

* CommandCatalogCreatedEvent — Raised when a new command-catalog is created
* CommandCatalogUpdatedEvent — Raised when command-catalog metadata is updated
* CommandCatalogStateChangedEvent — Raised when command-catalog lifecycle state transitions

## Specifications

* CommandCatalogSpecification — Validates command-catalog structure and completeness

## Domain Services

* CommandCatalogService — Domain operations for command-catalog management

## Invariants

* Must remain deterministic
* Must remain reusable across systems
* Must not contain business-specific rules

## Policy Dependencies

* None (core-system must be policy-agnostic)

## Integration Points

* All systems (shared usage layer)

## Lifecycle

Created → Active → Updated → Deprecated

## Notes

Core-system must remain minimal, pure, and reusable.
