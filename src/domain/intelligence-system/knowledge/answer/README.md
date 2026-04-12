# Domain: Answer

## Classification

intelligence-system

## Context

knowledge

## Purpose

Defines the structure of knowledge answers — the resolved responses to knowledge queries.

## Core Responsibilities

* Define the canonical structure for answer records
* Track lifecycle state of answer entries
* Emit domain events on creation, update, and state transitions

## Aggregate(s)

* AnswerAggregate

  * Represents a single answer record with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* AnswerId — Unique identifier for an answer instance

## Domain Events

* AnswerCreatedEvent — Raised when a new answer is created
* AnswerUpdatedEvent — Raised when answer metadata is updated
* AnswerStateChangedEvent — Raised when answer lifecycle state transitions

## Specifications

* AnswerSpecification — Validates answer structure and completeness

## Domain Services

* AnswerService — Domain operations for answer management

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
