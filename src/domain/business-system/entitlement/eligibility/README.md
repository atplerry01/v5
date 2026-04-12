# Domain: Eligibility

## Classification

business-system

## Context

entitlement

## Purpose

Defines eligibility rules and evaluates whether a subject meets the conditions required for an entitlement. Evaluations are deterministic and based on explicit criteria.

## Core Responsibilities

* Define eligibility identity and criteria
* Evaluate eligibility based on deterministic conditions
* Track eligibility evaluation outcome (Eligible or Ineligible)
* Enforce that evaluation is one-time per eligibility instance

## Aggregate(s)

* EligibilityAggregate
  * Manages the lifecycle and evaluation of an eligibility determination

## Value Objects

* EligibilityId — Unique identifier for an eligibility instance
* EligibilityStatus — Enum for lifecycle state (Pending, Eligible, Ineligible)
* SubjectId — Reference to the subject being evaluated

## Domain Events

* EligibilityCreatedEvent — Raised when a new eligibility evaluation is created
* EligibilityEvaluatedEligibleEvent — Raised when subject is determined eligible
* EligibilityEvaluatedIneligibleEvent — Raised when subject is determined ineligible

## Specifications

* CanEvaluateSpecification — Only Pending eligibilities can be evaluated
* IsEligibleSpecification — Checks if eligibility status is Eligible

## Domain Services

* EligibilityService — Domain operations for eligibility management

## Errors

* MissingId — EligibilityId is required
* MissingSubjectId — SubjectId is required
* MissingCriteria — CriteriaDescription is required
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyEvaluated — Eligibility already evaluated

## Invariants

* EligibilityId must not be null/default
* SubjectId must not be null/default
* CriteriaDescription must not be null or empty
* EligibilityStatus must be a defined enum value
* Evaluation is deterministic — no randomness
* Cannot be both eligible and ineligible simultaneously (enforced by enum)
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* allocation (eligibility gates allocation decisions)
* entitlement-grant (eligibility determines grant permissibility)

## Status

**S4 — Invariants + Specifications Complete**
