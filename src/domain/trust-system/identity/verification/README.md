# Domain: Verification

## Classification

trust-system

## Context

identity

## Domain

verification

## Boundary

This domain defines verification state structure only and contains no verification execution logic.

## Purpose

Defines verification state -- the structured assessment of identity claims. Captures whether a verification has been initiated and whether it passed or failed. Does NOT perform verification execution or make external calls.

## Core Responsibilities

* Define the structural state of a verification (Initiated, Passed, Failed)
* Enforce valid state transitions (terminal lifecycle: Initiated -> Passed | Failed)
* Emit domain events on state changes

## Aggregate(s)

* VerificationAggregate
  * Factory: Initiate(id, subject) -- creates a verification in Initiated status
  * Transitions: Pass(), Fail() -- terminal transitions from Initiated
  * Enforces invariants: non-empty id and subject

## Entities

* None

## Value Objects

* VerificationId -- Strongly-typed, validated Guid identifier
* VerificationStatus -- Enum: Initiated, Passed, Failed
* VerificationSubject -- Record struct with IdentityReference (Guid, non-empty) and ClaimType (string, non-empty)

## Domain Events

* VerificationInitiatedEvent(VerificationId, VerificationSubject)
* VerificationPassedEvent(VerificationId)
* VerificationFailedEvent(VerificationId)

## Specifications

* CanPassSpecification -- Satisfied when status is Initiated
* CanFailSpecification -- Satisfied when status is Initiated

## Domain Services

* VerificationService -- Empty; verification execution belongs outside this boundary

## Errors

* MissingId -- Verification identifier must not be empty
* MissingSubject -- Subject must have non-empty identity reference and claim type
* InvalidStateTransition(status, action) -- Cannot perform action in current status

## Invariants

* VerificationId must not be default/empty
* VerificationSubject must not be default/empty
* State transitions are terminal: only Initiated status allows Pass or Fail

## Lifecycle

TERMINAL: Initiated -> Passed | Failed. No further transitions after terminal state.

## Policy Dependencies

* Verification methods and evidence requirements (WHYCEPOLICY controlled at runtime)

## Integration Points

* Identity (WhyceID) -- Subject of verification via VerificationSubject.IdentityReference
