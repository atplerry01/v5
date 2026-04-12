# Domain: Contract (Integration)

## Classification

business-system

## Context

integration

## Purpose

Manages integration contracts — the formal schema definitions that govern how systems interact. An integration contract defines the structure and rules of external communication, tracked through drafting, activation, and termination.

## Core Responsibilities

* Define integration contract identity and schema
* Track contract lifecycle state (Draft -> Active -> Terminated)
* Enforce schema definition before activation
* Ensure terminated contracts cannot be reused

## Aggregate(s)

* ContractAggregate
  * Manages the lifecycle and integrity of an integration contract

## Entities

* ContractSchema — Defines the structure of the integration contract (SchemaId, SchemaName, SchemaDefinition)

## Value Objects

* ContractId — Unique identifier for a contract instance
* ContractStatus — Enum for lifecycle state (Draft, Active, Terminated)
* ContractSchemaId — Reference to the schema definition

## Domain Events

* ContractCreatedEvent — Raised when a new contract is drafted
* ContractActivatedEvent — Raised when the contract is activated
* ContractTerminatedEvent — Raised when the contract is terminated

## Specifications

* CanActivateSpecification — Only Draft contracts can be activated
* CanTerminateSpecification — Only Active contracts can be terminated
* IsActiveSpecification — Checks if contract is currently active

## Domain Services

* ContractService — Domain operations for contract management

## Errors

* MissingId — ContractId is required
* MissingSchema — ContractSchema is required
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyActive — Contract already in Active state
* AlreadyTerminated — Contract already in Terminated state

## Invariants

* ContractId must not be null/default
* ContractSchema must not be null
* ContractStatus must be a defined enum value
* Cannot activate without schema (enforced by entity validation)
* Terminated contracts are terminal
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* endpoint (contracts define endpoint interaction rules)
* provider (contracts govern provider communication)

## Status

**S4 — Invariants + Specifications Complete**
