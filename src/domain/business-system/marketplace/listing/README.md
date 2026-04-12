# Domain: Listing

## Classification

business-system

## Context

marketplace

## Boundary Statement

This domain defines marketplace listing identity and structure. No pricing, matching, or execution logic.

## Lifecycle Pattern

**REVERSIBLE** — Draft → Active ↔ Inactive. Can reactivate from Inactive.

## Purpose

Defines a marketplace listing — an available offering posted by an owner referencing a specific item or resource.

## Core Responsibilities

* Define listing identity, ownership, and item reference
* Track listing lifecycle state (Draft → Active ↔ Inactive)
* Enforce ownership and item reference integrity
* Ensure no implicit state changes

## Aggregate(s)

* ListingAggregate
  * Manages the lifecycle and integrity of a marketplace listing

## Value Objects

* ListingId — Unique identifier for a listing instance
* ListingStatus — Enum for lifecycle state (Draft, Active, Inactive)
* ListingOwnerId — Reference to the listing owner
* ListingItemReference — Reference to the listed item or resource

## Domain Events

* ListingCreatedEvent — Raised when a new listing is defined
* ListingActivatedEvent — Raised when the listing becomes active
* ListingDeactivatedEvent — Raised when the listing is deactivated (reversible)

## Specifications

* CanActivateListingSpecification — Draft or Inactive listings can be activated
* CanDeactivateListingSpecification — Only Active listings can be deactivated
* IsActiveListingSpecification — Checks if listing is currently active

## Domain Services

* ListingService — Domain operations for listing management

## Errors

* MissingId — ListingId is required
* MissingOwnerId — ListingOwnerId is required
* MissingItemReference — ListingItemReference is required
* InvalidStateTransition — Attempted transition not allowed from current status

## Invariants

* ListingId must not be null/default
* ListingOwnerId must not be null/default (must have owner)
* ListingItemReference must not be null/default (must define item/resource reference)
* ListingTitle must not be null or empty
* ListingStatus must be a defined enum value
* Activate/Deactivate governed by specifications (reversible)

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* offer (offers reference listings)
* order (orders may reference listings)
* match (matches may reference listings)

## Status

**S4 — Invariants + Specifications Complete**
