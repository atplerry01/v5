# Domain: Geofence

## Classification

intelligence-system

## Context

geo

## Purpose

Defines the structure of geofence definitions — the virtual geographic boundaries used for location-based intelligence.

## Core Responsibilities

* Define geofence structure and metadata
* Enforce geofence invariants and validation rules
* Emit domain events on geofence lifecycle transitions

## Aggregate(s)

* GeofenceAggregate

  * Root aggregate representing a geofence with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* GeofenceId — Unique identifier for a geofence instance

## Domain Events

* GeofenceCreatedEvent — Raised when a new geofence is created
* GeofenceUpdatedEvent — Raised when geofence metadata is updated
* GeofenceStateChangedEvent — Raised when geofence lifecycle state transitions

## Specifications

* GeofenceSpecification — Validates geofence structure and completeness

## Domain Services

* GeofenceService — Domain operations for geofence management

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
