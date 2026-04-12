# Domain: Receipt

## Classification

business-system

## Context

integration

## Purpose

Defines the structure of integration receipts — the acknowledgment records confirming successful integration message delivery.

## Core Responsibilities

* Define the structural identity and metadata of integration receipts
* Track receipt lifecycle states and transitions
* Maintain relationships between receipts and other integration entities

## Aggregate(s)

* Receipt

  * Represents an acknowledgment record confirming successful integration message delivery

## Entities

* None

## Value Objects

* ReceiptId — Unique identifier for a receipt instance

## Domain Events

* ReceiptCreatedEvent — Raised when a new receipt is created
* ReceiptUpdatedEvent — Raised when receipt metadata is updated
* ReceiptStateChangedEvent — Raised when receipt lifecycle state transitions

## Specifications

* ReceiptSpecification — Validates receipt structure and completeness

## Domain Services

* ReceiptService — Domain operations for receipt management

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
