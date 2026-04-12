# Domain: Request

## Classification

trust-system

## Context

access

## Purpose

Represents the domain responsible for managing access requests — formal petitions by principals to obtain access rights, permissions, or role assignments that require approval before being granted.

## Core Responsibilities

* Capture and manage access request submissions
* Track request lifecycle through approval workflows
* Emit events when requests are submitted, approved, or denied

## Aggregate(s)

* RequestAggregate
  * Enforces invariants around request submission and state transitions
  * Validates request eligibility before committing changes

## Entities

* None

## Value Objects

* RequestId — Strongly-typed identifier for an access request

## Domain Events

* RequestCreatedEvent — Raised when a new access request is submitted
* RequestStateChangedEvent — Raised when request state transitions (e.g., pending → approved)
* RequestUpdatedEvent — Raised when an existing request is modified

## Specifications

* RequestSpecification — Validates request eligibility and completeness

## Domain Services

* RequestService — Coordinates request submission and evaluation logic

## Invariants

* A request must reference a valid requester and target permission or role
* Requests cannot be approved without passing policy evaluation
* State transitions must pass pre-change validation

## Policy Dependencies

* Approval workflows, auto-approval rules, request expiry durations (WHYCEPOLICY controlled)

## Integration Points

* Identity (WhyceID) — Requester and approver resolution
* Permission — Target permissions being requested
* Role — Target roles being requested
* Grant — Approved requests result in grants
* Governance — Audit trail of request decisions

## Lifecycle

Submitted → Pending Review → Approved | Denied | Expired | Cancelled. All transitions emit domain events and enforce invariants.

## Notes

Requests represent the demand side of access control. They create a reviewable, auditable record of who asked for what and why. Approval policies and auto-grant rules are WHYCEPOLICY controlled.
