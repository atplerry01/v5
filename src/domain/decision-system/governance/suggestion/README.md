# Domain: Suggestion

## Classification

decision-system

## Context

governance

## Purpose

Manages governance suggestions submitted for consideration by decision-making bodies.

## Core Responsibilities

* Suggestion capture and classification
* Suggestion evaluation tracking
* Suggestion disposition recording

## Aggregate(s)

* SuggestionAggregate
  * Enforces suggestion lifecycle

## Entities

* None

## Value Objects

* SuggestionId

## Domain Events

* SuggestionCreatedEvent
* SuggestionStateChangedEvent
* SuggestionUpdatedEvent

## Specifications

* SuggestionSpecification — suggestion validation rules

## Domain Services

* SuggestionService

## Invariants

* Suggestions must have an identified submitter
* Suggestions must be evaluated within defined timeframes
* Suggestion dispositions must be communicated
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

submitted → classified → under-evaluation → accepted/deferred/rejected → closed

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
