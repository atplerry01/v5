# Domain: Role

## Classification

trust-system

## Context

access

## Purpose

Represents the domain responsible for defining and managing roles — named collections of permissions that can be assigned to principals to simplify access control management.

## Core Responsibilities

* Define roles as logical groupings of permissions
* Manage role assignments and hierarchies
* Emit events when roles are created, modified, or assigned

## Aggregate(s)

* RoleAggregate
  * Enforces invariants around role definition and permission composition
  * Validates role structure before committing changes

## Entities

* None

## Value Objects

* RoleId — Strongly-typed identifier for a role

## Domain Events

* RoleCreatedEvent — Raised when a new role is defined
* RoleStateChangedEvent — Raised when role state transitions
* RoleUpdatedEvent — Raised when an existing role is modified

## Specifications

* RoleSpecification — Validates role definition and composition criteria

## Domain Services

* RoleService — Coordinates role management and assignment logic

## Invariants

* Roles must have unique identifiers within their scope
* Role permission composition must not create circular hierarchies
* State transitions must pass pre-change validation

## Policy Dependencies

* Role hierarchy depth limits, maximum permissions per role, assignment constraints (WHYCEPOLICY controlled)

## Integration Points

* Permission — Roles compose permissions
* Grant — Role assignments may be expressed as grants
* Authorization — Roles are resolved during authorization evaluation
* Identity (WhyceID) — Principals to whom roles are assigned
* Governance — Role definitions subject to governance audit

## Lifecycle

Created → Active → Updated → Deprecated | Removed. All transitions emit domain events and enforce invariants.

## Notes

Roles provide a level of indirection between principals and permissions, enabling scalable access management. Role inheritance, mutual exclusion, and separation-of-duty constraints are WHYCEPOLICY controlled.
