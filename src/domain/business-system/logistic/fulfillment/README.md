# Domain: Fulfillment

## Classification

business-system

## Context

logistic

## Purpose

Defines the completion declaration for a shipment process. Fulfillment represents the terminal state where a shipment's logistic obligations have been satisfied.

## Boundary Statement

This domain defines logistic coordination contracts only and contains no execution logic.

## Core Responsibilities

* Define fulfillment completion declarations referencing shipments
* Enforce that fulfillment is explicit and non-partial
* Maintain shipment reference integrity on fulfillment records

## Aggregate(s)

* FulfillmentAggregate
  * Event-sourced root entity governing a fulfillment record
  * Tracks completion state for a referenced shipment
  * Enforces invariants: valid id, valid shipment reference, valid status

## Entities

* None

## Value Objects

* FulfillmentId — Unique typed identifier for a fulfillment instance
* FulfillmentStatus — Lifecycle state (Created, Completed)
* ShipmentReference — Reference to the shipment being fulfilled

## Domain Events

* FulfillmentCreatedEvent — Raised when a new fulfillment is created (carries FulfillmentId, ShipmentReference)
* FulfillmentCompletedEvent — Raised when fulfillment reaches terminal completion (carries FulfillmentId)
* FulfillmentStateChangedEvent — Raised when fulfillment lifecycle state transitions (carries previous and new status)
* FulfillmentUpdatedEvent — Raised when fulfillment metadata is updated

## Specifications

* FulfillmentSpecification — Validates fulfillment structure: id and shipment reference must be non-default
* IsCompletedSpecification — Checks whether fulfillment status is Completed

## Domain Services

* FulfillmentService — Reserved for domain operations that span beyond a single aggregate

## Invariants

* Must reference a valid shipment (ShipmentReference != default)
* Must have a valid FulfillmentId
* Cannot be partially implicit — completion is explicit via Complete()
* Cannot be completed more than once (terminal state)
* Status must be a defined enum value

## Lifecycle Pattern

TERMINAL: Created -> Completed

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* Shipment domain (read-only reference via ShipmentReference value object)

## Notes

No fulfillment execution logic. No delivery logic. No real-time updates. No external dependencies. Deterministic only.
