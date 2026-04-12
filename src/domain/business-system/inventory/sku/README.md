# Domain: Sku

## Classification

business-system

## Context

inventory

## Purpose

Defines the structure of SKUs (Stock Keeping Units) — the unique product identifiers used for inventory tracking. SKU is a critical domain that must uniquely identify products and cannot be duplicated.

## Core Responsibilities

* Define the structural representation of SKUs
* Enforce unique product identity through SkuCode
* Maintain SKU identity and lifecycle integrity
* Emit domain events on SKU lifecycle transitions

## Aggregate(s)

* SkuAggregate
  * Represents the root entity for a SKU, encapsulating its identity, code, and lifecycle rules

## Entities

* None

## Value Objects

* SkuId — Unique identifier for a SKU instance
* SkuCode — Unique product code (critical — no duplicates)
* SkuStatus — Lifecycle state (Active, Discontinued)

## Domain Events

* SkuCreatedEvent — Raised when a new SKU is created
* SkuDiscontinuedEvent — Raised when a SKU is discontinued
* SkuUpdatedEvent — Raised when SKU metadata is updated
* SkuStateChangedEvent — Raised when SKU lifecycle state transitions

## Specifications

* CanDiscontinueSkuSpecification — Guards transition from Active to Discontinued
* SkuSpecification — Validates SKU structure and completeness

## Domain Services

* SkuService — Coordination placeholder for SKU domain operations

## Invariants

* SKU must have a valid identity (non-empty SkuId)
* SKU must have a unique product code (non-empty SkuCode)
* SKU cannot be duplicated — identity uniqueness is critical
* Once discontinued, SKU cannot be reactivated (TERMINAL)
* Status must be a defined enum value

## Boundary Statement

This domain owns SKU identity definition only. Product details, pricing, and categorization belong to other domains. No implicit stock changes. Identity uniqueness is enforced at the domain level via SkuCode.

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* item (SKU identifies items)
* stock (SKU referenced in stock records)
* structural-system

## Lifecycle

Active → Discontinued

**Pattern: TERMINAL** — Once discontinued, SKU cannot be reactivated.

## Status

**S4 — Invariants + Specifications Complete**
