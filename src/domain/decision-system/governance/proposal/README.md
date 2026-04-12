# Domain: Proposal

## Classification

decision-system

## Context

governance

## Purpose

Manages governance proposals submitted for collective decision-making.

## Core Responsibilities

* Proposal submission and validation
* Proposal deliberation tracking
* Proposal outcome recording

## Aggregate(s)

* ProposalAggregate
  * Enforces proposal lifecycle

## Entities

* None

## Value Objects

* ProposalId

## Domain Events

* ProposalCreatedEvent
* ProposalStateChangedEvent
* ProposalUpdatedEvent

## Specifications

* ProposalSpecification — proposal validation rules

## Domain Services

* ProposalService

## Invariants

* Proposals must have a defined sponsor
* Proposals must follow submission procedures
* Proposal outcomes must be recorded with rationale
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

drafted → submitted → deliberating → voted → accepted/rejected → archived

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
