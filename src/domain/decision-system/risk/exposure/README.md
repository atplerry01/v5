# Domain: Exposure

## Classification

decision-system

## Context

risk

## Purpose

Tracks and quantifies risk exposure levels resulting from identified risks.

## Core Responsibilities

* Exposure calculation and tracking
* Exposure threshold monitoring
* Exposure trend analysis recording

## Aggregate(s)

* ExposureAggregate
  * Enforces exposure lifecycle

## Entities

* None

## Value Objects

* ExposureId

## Domain Events

* ExposureCreatedEvent
* ExposureStateChangedEvent
* ExposureUpdatedEvent

## Specifications

* ExposureSpecification — exposure validation rules

## Domain Services

* ExposureService

## Invariants

* Exposure must be quantified against defined metrics
* Exposure levels must be current
* Exposure breaches must trigger alerts
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

calculated → recorded → monitored → breached/within-tolerance → recalculated

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
