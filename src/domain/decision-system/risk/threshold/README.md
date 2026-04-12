# Domain: Threshold

## Classification

decision-system

## Context

risk

## Purpose

Defines and manages risk thresholds that trigger alerts and escalation.

## Core Responsibilities

* Threshold definition and calibration
* Threshold breach detection
* Threshold adjustment tracking

## Aggregate(s)

* ThresholdAggregate
  * Enforces threshold lifecycle

## Entities

* None

## Value Objects

* ThresholdId

## Domain Events

* ThresholdCreatedEvent
* ThresholdStateChangedEvent
* ThresholdUpdatedEvent

## Specifications

* ThresholdSpecification — threshold validation rules

## Domain Services

* ThresholdService

## Invariants

* Thresholds must have defined trigger levels
* Threshold changes must be approved
* Threshold breaches must generate alerts
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

defined → calibrated → active → breached/within-tolerance → recalibrated → archived

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
