# Domain: Trust

## Classification

trust-system

## Context

identity

## Purpose

Represents the domain responsible for managing trust scores and trust state — the quantified measure of confidence in an identity's reliability, behaviour history, and verified status within the trust system.

## Core Responsibilities

* Calculate and maintain trust scores for identities
* Track trust-affecting events and adjust scores accordingly
* Emit events when trust levels change

## Aggregate(s)

* TrustAggregate
  * Enforces invariants around trust score boundaries and transitions
  * Validates trust state before committing changes

## Entities

* None

## Value Objects

* TrustId — Strongly-typed identifier for a trust record

## Domain Events

* TrustCreatedEvent — Raised when a trust record is established for an identity
* TrustStateChangedEvent — Raised when trust score transitions between levels
* TrustUpdatedEvent — Raised when trust score is recalculated

## Specifications

* TrustSpecification — Validates trust score criteria and threshold rules

## Domain Services

* TrustService — Coordinates trust score calculation and evaluation logic

## Invariants

* Trust scores must remain within defined minimum and maximum bounds
* Trust score changes must be traceable to specific trust-affecting events
* Trust level transitions must pass policy-defined thresholds

## Policy Dependencies

* Trust score ranges, level thresholds, decay rates, boost/penalty rules (WHYCEPOLICY controlled)

## Integration Points

* Identity (WhyceID) — Trust subject
* Verification — Verification outcomes influence trust scores
* Identity Graph — Graph position may factor into trust calculation
* Authorization (access context) — Trust level may gate authorization decisions
* Economic (economic-system) — Trust may influence economic capabilities
* Governance — Trust score audit trail

## Lifecycle

Initialised → Scored → Updated → Degraded | Elevated. All transitions emit domain events and enforce invariants.

## Notes

Trust is the quantitative backbone of the trust-system classification. It provides the measurable signal that other domains use for policy decisions. Trust scoring algorithms and thresholds are WHYCEPOLICY controlled. This domain models trust state, not the scoring algorithm itself (which is an engine concern).
