# Domain: Provider

## Classification

business-system

## Context

integration

## Purpose

Manages external provider registrations and lifecycle. A provider represents an external system or service that the platform integrates with, tracked through registration, activation, and suspension.

## Core Responsibilities

* Define provider identity and profile metadata
* Track provider lifecycle state (Registered → Active → Suspended)
* Enforce profile configuration before activation
* Ensure suspended providers cannot be used

## Aggregate(s)

* ProviderAggregate
  * Manages the lifecycle and integrity of an external provider

## Entities

* ProviderProfile — Provider metadata (ConfigId, ProviderName, ProviderType)

## Value Objects

* ProviderId — Unique identifier for a provider instance
* ProviderStatus — Enum for lifecycle state (Registered, Active, Suspended)
* ProviderConfigId — Reference to the provider configuration

## Domain Events

* ProviderCreatedEvent — Raised when a new provider is registered
* ProviderActivatedEvent — Raised when the provider is activated
* ProviderSuspendedEvent — Raised when the provider is suspended

## Specifications

* CanActivateSpecification — Only Registered providers can be activated
* CanSuspendSpecification — Only Active providers can be suspended
* IsActiveSpecification — Checks if provider is currently active

## Domain Services

* ProviderService — Domain operations for provider management

## Errors

* MissingId — ProviderId is required
* MissingProfile — ProviderProfile is required
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyActive — Provider already in Active state
* AlreadySuspended — Provider already in Suspended state

## Invariants

* ProviderId must not be null/default
* ProviderProfile must not be null
* ProviderStatus must be a defined enum value
* Cannot activate without profile (enforced by entity validation)
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* endpoint (providers expose endpoints)
* contract (providers operate under integration contracts)
* connector (providers are reached via connectors)

## Status

**S4 — Invariants + Specifications Complete**
