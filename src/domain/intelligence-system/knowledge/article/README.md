# Domain: Article

## Classification

intelligence-system

## Context

knowledge

## Purpose

Defines the structure of knowledge articles — the authored content entries in the knowledge base.

## Core Responsibilities

* Define the canonical structure for article records
* Track lifecycle state of article entries
* Emit domain events on creation, update, and state transitions

## Aggregate(s)

* ArticleAggregate

  * Represents a single article record with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* ArticleId — Unique identifier for an article instance

## Domain Events

* ArticleCreatedEvent — Raised when a new article is created
* ArticleUpdatedEvent — Raised when article metadata is updated
* ArticleStateChangedEvent — Raised when article lifecycle state transitions

## Specifications

* ArticleSpecification — Validates article structure and completeness

## Domain Services

* ArticleService — Domain operations for article management

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
