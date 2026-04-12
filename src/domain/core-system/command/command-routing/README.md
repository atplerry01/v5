# Domain: CommandRouting

## Classification

core-system

## Context

command

## Purpose

Defines the routing rules and targets for command delivery. Provides the structural mapping of commands to their handlers and destinations.

## Boundary

This domain defines command routing structure only and contains no dispatch or execution logic.

## Core Responsibilities

* Defining routing rules that map commands to their target handlers
* Managing handler and destination registrations for command delivery
* Providing structural routing metadata for dispatch infrastructure

## Aggregate(s)

* CommandRoutingAggregate

  * Factory: Define(id, rule) — creates a routing entry in Defined status
  * Transitions: Activate(), Disable()
  * Lifecycle: Defined -> Active -> Disabled (TERMINAL)

## Entities

* None

## Value Objects

* CommandRoutingId — Validated Guid identifier for a command-routing instance
* CommandRoutingStatus — Enum: Defined, Active, Disabled
* RoutingRule — Record struct with CommandName (string) and TargetHandler (string), both validated non-empty

## Domain Events

* CommandRoutingDefinedEvent(CommandRoutingId, RoutingRule) — Raised when a new command-routing is defined
* CommandRoutingActivatedEvent(CommandRoutingId) — Raised when a command-routing is activated
* CommandRoutingDisabledEvent(CommandRoutingId) — Raised when a command-routing is disabled

## Specifications

* CanActivateSpecification — status == Defined
* CanDisableSpecification — status == Active

## Domain Services

* CommandRoutingService — Empty (no domain operations required)

## Invariants

* Must remain deterministic
* Must remain reusable across systems
* Must not contain business-specific rules

## Policy Dependencies

* None (core-system must be policy-agnostic)

## Integration Points

* All systems (shared usage layer)

## Lifecycle

Defined -> Active -> Disabled (TERMINAL)

## Notes

Core-system must remain minimal, pure, and reusable.
