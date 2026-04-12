# Domain: Synonym

## Classification

intelligence-system

## Context

search

## Purpose

Defines the structure of search synonym records — the equivalent terms used to expand and improve search matching.

## Core Responsibilities

* Model synonym structures and term equivalences
* Track synonym groups and expansion rules
* Maintain synonym lifecycle and state transitions

## Aggregate(s)

* SynonymAggregate

  * Encapsulates the lifecycle and invariants of a search synonym record

## Entities

* None

## Value Objects

* SynonymId — Unique identifier for a synonym instance

## Domain Events

* SynonymCreatedEvent — Raised when a new synonym is created
* SynonymUpdatedEvent — Raised when synonym metadata is updated
* SynonymStateChangedEvent — Raised when synonym lifecycle state transitions

## Specifications

* SynonymSpecification — Validates synonym structure and completeness

## Domain Services

* SynonymService — Domain operations for synonym management

## Invariants

* Intelligence artifacts must be deterministic and traceable
* No execution logic allowed
* No inference logic allowed

## Policy Dependencies

* Governance or usage constraints may be governed by WHYCEPOLICY

## Integration Points

* decision-system (consumes insights)
* trust-system (signals influence trust)
* economic-system (signals influence risk)

## Lifecycle

Created → Updated → Evaluated → Archived

## Notes

This domain represents intelligence structure ONLY. All AI/ML execution is external (T3I layer).
