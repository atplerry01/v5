# Domain: Webhook

## Classification

business-system

## Context

integration

## Boundary Statement

This domain defines interaction contracts only and contains no transport or execution logic.

## Lifecycle Pattern

**REVERSIBLE** --- Can toggle between Active and Disabled states.

## Purpose

Manages webhook definition contracts for integration. A webhook defines the endpoint target and metadata for outbound notifications, without containing any HTTP transport or delivery logic.

## Core Responsibilities

* Define webhook identity and endpoint definition
* Track webhook lifecycle state (Defined -> Active <-> Disabled)
* Enforce definition presence before activation
* Ensure no HTTP calls or transport logic in domain

## Aggregate(s)

* WebhookAggregate
  * Manages the lifecycle and integrity of a webhook definition

## Entities

* WebhookDefinition --- Webhook structure (EndpointId, WebhookName, TargetUri)

## Value Objects

* WebhookId --- Unique identifier for a webhook instance
* WebhookStatus --- Enum for lifecycle state (Defined, Active, Disabled)
* WebhookEndpointId --- Reference to the webhook endpoint

## Domain Events

* WebhookCreatedEvent --- Raised when a new webhook is defined
* WebhookActivatedEvent --- Raised when the webhook is activated
* WebhookDisabledEvent --- Raised when the webhook is disabled

## Specifications

* CanActivateSpecification --- Defined or Disabled webhooks can be activated
* CanDisableSpecification --- Only Active webhooks can be disabled
* IsActiveSpecification --- Checks if webhook is currently active

## Domain Services

* WebhookService --- Domain operations for webhook management

## Errors

* MissingId --- WebhookId is required
* MissingDefinition --- WebhookDefinition is required
* InvalidStateTransition --- Attempted transition not allowed from current status
* AlreadyActive --- Webhook already in Active state
* AlreadyDisabled --- Webhook already in Disabled state

## Invariants

* WebhookId must not be null/default
* WebhookDefinition must not be null
* WebhookStatus must be a defined enum value
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* endpoint (webhooks target integration endpoints)
* contract (webhooks operate under integration contracts)

## Status

**S4 --- Invariants + Specifications Complete**
