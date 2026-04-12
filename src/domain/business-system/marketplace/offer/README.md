# Domain: Offer

## Classification

business-system

## Context

marketplace

## Boundary Statement

This domain defines marketplace offer structure and terms. No pricing algorithms, matching, or execution logic.

## Lifecycle Pattern

**REVERSIBLE** — Pending → Accepted | Rejected | Withdrawn. Only Pending offers can transition.

## Purpose

Defines a marketplace offer — a proposal to transact against a listing, including the terms of the proposal.

## Core Responsibilities

* Define offer identity, listing reference, and terms
* Track offer lifecycle state (Pending → Accepted | Rejected | Withdrawn)
* Enforce listing reference and terms integrity
* Ensure no implicit state changes

## Aggregate(s)

* OfferAggregate
  * Manages the lifecycle and integrity of a marketplace offer

## Value Objects

* OfferId — Unique identifier for an offer instance
* OfferStatus — Enum for lifecycle state (Pending, Accepted, Rejected, Withdrawn)
* OfferListingReference — Reference to the listing this offer targets
* OfferTerms — Encapsulates the terms of the offer proposal

## Domain Events

* OfferCreatedEvent — Raised when a new offer is submitted
* OfferAcceptedEvent — Raised when the offer is accepted
* OfferRejectedEvent — Raised when the offer is rejected
* OfferWithdrawnEvent — Raised when the offer is withdrawn by the proposer

## Specifications

* CanAcceptOfferSpecification — Only Pending offers can be accepted
* CanRejectOfferSpecification — Only Pending offers can be rejected
* CanWithdrawOfferSpecification — Only Pending offers can be withdrawn
* IsPendingOfferSpecification — Checks if offer is currently pending

## Domain Services

* OfferService — Domain operations for offer management

## Errors

* MissingId — OfferId is required
* MissingListingReference — OfferListingReference is required
* MissingTerms — OfferTerms are required
* InvalidStateTransition — Attempted transition not allowed from current status

## Invariants

* OfferId must not be null/default
* OfferListingReference must not be null/default (must reference listing)
* OfferTerms must not be null (must include terms)
* OfferStatus must be a defined enum value
* Accept/Reject/Withdraw governed by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* listing (offers reference listings)
* order (accepted offers may lead to orders)
* match (accepted offers may produce matches)

## Status

**S4 — Invariants + Specifications Complete**
