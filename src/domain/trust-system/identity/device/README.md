# Domain: Device

## Classification

trust-system

## Context

identity

## Purpose

Represents the domain responsible for managing device registrations — the trusted endpoints (physical or virtual) that identities use to interact with the system, enabling device-bound trust and access policies.

## Core Responsibilities

* Register and manage devices associated with identities
* Track device trust status and binding
* Emit events when devices are registered, trusted, or revoked

## Aggregate(s)

* DeviceAggregate
  * Enforces invariants around device registration and trust transitions
  * Validates device binding before committing changes

## Entities

* None

## Value Objects

* DeviceId — Strongly-typed identifier for a device record

## Domain Events

* DeviceCreatedEvent — Raised when a new device is registered
* DeviceStateChangedEvent — Raised when device trust state transitions
* DeviceUpdatedEvent — Raised when device metadata is modified

## Specifications

* DeviceSpecification — Validates device registration and trust criteria

## Domain Services

* DeviceService — Coordinates device registration and trust management logic

## Invariants

* A device must be bound to at least one identity
* Device trust status must be explicitly established, not assumed
* Revoked devices must not be used for session creation

## Policy Dependencies

* Maximum devices per identity, device trust expiry, re-registration rules (WHYCEPOLICY controlled)

## Integration Points

* Identity (WhyceID) — Device owner resolution
* Credential — Device-bound credentials
* Session (access context) — Sessions may be device-scoped
* Governance — Device registration audit trail

## Lifecycle

Registered → Trusted → Updated → Untrusted | Revoked. All transitions emit domain events and enforce invariants.

## Notes

Devices represent trust anchors in the physical world. Device trust is orthogonal to identity trust — a trusted identity on an untrusted device may receive reduced capabilities. Device policies are WHYCEPOLICY controlled.
