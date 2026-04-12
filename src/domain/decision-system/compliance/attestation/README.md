# Domain: Attestation

## Classification

decision-system

## Context

compliance

## Purpose

Manages compliance attestation decisions confirming adherence to requirements.

## Core Responsibilities

* Attestation creation and validation
* Compliance confirmation recording
* Attestation expiry tracking

## Aggregate(s)

* AttestationAggregate
  * Enforces attestation lifecycle

## Entities

* None

## Value Objects

* AttestationId

## Domain Events

* AttestationCreatedEvent
* AttestationStateChangedEvent
* AttestationUpdatedEvent

## Specifications

* AttestationSpecification — attestation validation rules

## Domain Services

* AttestationService

## Invariants

* Attestations must reference specific obligations
* Attestations must have a validity period
* Expired attestations must not be treated as active
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

requested → evaluated → attested → active → expired → renewed/archived

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
