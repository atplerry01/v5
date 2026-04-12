# Domain: Catalog

## Classification: business-system
## Context: marketplace
## Status: S4 — Invariants + Specifications Complete

## Purpose

Defines the structure of marketplace catalogs — organized groupings of listings with a defined structure and category. A catalog provides the structural container for marketplace offerings.

## Boundary Statement

This domain defines catalog **structure only**. No pricing logic, financial transfers, or external system interaction is permitted. Catalog content management and search are handled by other domains.

## Aggregate(s)

* CatalogAggregate — Event-sourced aggregate managing catalog lifecycle and invariants

## Value Objects

* CatalogId — Unique identifier for a catalog instance (validated non-empty)
* CatalogStatus — Lifecycle state enum (Draft, Published, Archived)
* CatalogStructure — Structure definition with name and category (validated non-empty)

## Domain Events

* CatalogCreatedEvent(CatalogId, CatalogStructure) — Raised when a new catalog is created
* CatalogPublishedEvent(CatalogId) — Raised when a catalog is published
* CatalogArchivedEvent(CatalogId) — Raised when a catalog is archived

## Specifications

* CanPublishSpecification — Validates catalog can transition from Draft to Published
* CanArchiveSpecification — Validates catalog can transition from Published to Archived

## Domain Services

* CatalogService — Reserved for domain operations

## Errors

* CatalogErrors.MissingId — CatalogId is required
* CatalogErrors.MissingStructure — Catalog must have a structure definition
* CatalogErrors.InvalidStateTransition — Illegal lifecycle transition attempted

## Invariants

* Catalog must have a valid, non-empty CatalogId at all times
* Catalog must have a structure definition (name + category)
* Catalog must group listings (structural container)
* Status must be a defined CatalogStatus value
* Immutable after publication — no structural changes once Published
* No financial or execution logic allowed

## Lifecycle Pattern: TERMINAL

```
Draft → Published → Archived
```

* Draft: initial state after creation
* Published: catalog made available (immutable after this point)
* Archived: catalog permanently retired (terminal — no reversal)

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed.
