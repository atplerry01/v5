# Domain: Alert

## Classification

decision-system

## Context

risk

## Purpose

Manages risk alerts raised when thresholds are breached or risk conditions are detected.

## Core Responsibilities

* Alert generation and classification
* Alert severity assessment
* Alert escalation tracking

## Aggregate(s)

* AlertAggregate
  * Enforces alert lifecycle

## Entities

* None

## Value Objects

* AlertId

## Domain Events

* AlertCreatedEvent
* AlertStateChangedEvent
* AlertUpdatedEvent

## Specifications

* AlertSpecification — alert validation rules

## Domain Services

* AlertService

## Invariants

* Alerts must reference a risk source
* Alerts must be classified by severity
* Alerts must be acknowledged or escalated within defined timeframes
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

triggered → classified → acknowledged → escalated/resolved → closed

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
