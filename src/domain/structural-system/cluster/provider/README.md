# Domain: Provider

## Classification

structural-system

## Context

cluster

## Boundary

This domain defines service provider structure only and contains no service execution logic. Provider MUST reference a ClusterId.

## Purpose

Defines the structure of service providers linked to a cluster -- the entities that supply services within a cluster's scope.

## Core Responsibilities

* Define the structural identity and metadata of service providers
* Track provider lifecycle states and transitions
* Enforce ClusterId reference on every provider

## Aggregate(s)

* Provider

  * Represents a service provider linked to a cluster
  * Factory: Register(id, profile)
  * Transitions: Activate(), Suspend()

## WHEN-NEEDED folders

- no `entity/` — aggregate has no child entities.
- no `service/` — aggregate has no cross-aggregate coordination logic.

## Value Objects

* ProviderId -- Validated Guid identifier for a provider instance
* ProviderStatus -- Enum: Registered, Active, Suspended
* ProviderProfile -- Record struct with ClusterReference (Guid, non-empty) and ProviderName (string, non-empty)

## Domain Events

* ProviderRegisteredEvent(ProviderId, ProviderProfile) -- Raised when a new provider is registered
* ProviderActivatedEvent(ProviderId) -- Raised when a provider is activated
* ProviderSuspendedEvent(ProviderId) -- Raised when a provider is suspended

## Specifications

* CanActivateSpecification -- status == Registered
* CanSuspendSpecification -- status == Active

## Errors

* MissingId -- ProviderId is required
* MissingProfile -- ProviderProfile is required
* InvalidStateTransition(status, action) -- InvalidOperationException

## Invariants

* ProviderId must not be empty
* ProviderProfile must be provided
* ClusterReference within ProviderProfile must not be empty
* State transitions must follow lifecycle rules
* No service execution logic allowed

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system/cluster/cluster (ClusterId reference)

## Lifecycle

REVERSIBLE: Registered -> Active -> Suspended

## Notes

Structural-system defines structure only. No financial, execution, or workflow logic allowed.
