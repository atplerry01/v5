# Domain: Client

## Classification

business-system

## Context

integration

## Purpose

Manages external client definitions — outbound connection configurations for communicating with external systems. Tracks client lifecycle from registered through active to suspended. Requires credential references before activation. No secrets stored — credential references only.

## Core Responsibilities

* Define client identity and external identity reference
* Track client lifecycle state (Registered → Active → Suspended)
* Enforce credential-before-activation guard
* Suspended clients cannot initiate interactions

## Aggregate(s)

* ClientAggregate
  * Manages the lifecycle and credential inventory of an integration client

## Entities

* ClientCredential — Credential reference (CredentialId, CredentialType). No secrets stored.

## Value Objects

* ClientId — Unique identifier for a client instance
* ClientStatus — Enum for lifecycle state (Registered, Active, Suspended)
* ExternalClientId — Reference to the external system client identity

## Domain Events

* ClientCreatedEvent — Raised when a new client is registered
* ClientActivatedEvent — Raised when client is activated
* ClientSuspendedEvent — Raised when client is suspended

## Specifications

* CanActivateSpecification — Only Registered clients can be activated
* CanSuspendSpecification — Only Active clients can be suspended
* IsActiveSpecification — Checks if client is currently active

## Domain Services

* ClientService — Reserved for cross-aggregate coordination

## Errors

* MissingId — ClientId is required
* MissingExternalId — ExternalClientId is required
* CredentialRequired — Must have credential before activation
* InvalidStateTransition — Invalid state transition attempted
* AlreadyActive — Client already active
* AlreadySuspended — Client already suspended

## Invariants

* ClientId must not be null/default
* ExternalClientId must not be null/default
* ClientStatus must be a defined enum value
* Cannot activate without at least one credential reference
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* adapter (clients use adapters for translation)
* endpoint (clients connect to endpoints)

## Status

**S4 — Invariants + Specifications Complete**
