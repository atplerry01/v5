# Domain: Control

## Classification

decision-system

## Context

risk

## Purpose

Manages risk control decisions defining mitigating measures for identified risks.

## Core Responsibilities

* Control design and specification
* Control effectiveness evaluation
* Control implementation tracking

## Aggregate(s)

* ControlAggregate
  * Enforces control lifecycle

## Entities

* None

## Value Objects

* ControlId

## Domain Events

* ControlCreatedEvent
* ControlStateChangedEvent
* ControlUpdatedEvent

## Specifications

* ControlSpecification — control validation rules

## Domain Services

* ControlService

## Invariants

* Controls must reference specific risks
* Controls must have defined effectiveness criteria
* Control gaps must trigger escalation
* Decisions must be deterministic
* Decisions must be traceable
* Decisions must not bypass policy

## Policy Dependencies

* Decision rules governed by WHYCEPOLICY

## Integration Points

* constitutional-system (policy)
* trust-system (trust input)
* economic-system (impact)

## Lifecycle

designed → approved → implemented → monitored → reviewed → retired

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
