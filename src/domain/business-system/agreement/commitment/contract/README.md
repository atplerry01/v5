# Domain: Contract

## Classification

business-system

## Context

agreement

## Domain Responsibility

Models the root business entity representing a formal agreement between parties. Tracks contract lifecycle from draft through activation, suspension, and termination.

## Aggregate

* **ContractAggregate** — Root aggregate representing a formal contract.
  * Private constructor; created via `Create(ContractId)` factory method.
  * State transitions via `Activate()`, `Suspend()`, and `Terminate()` methods.
  * Manages `ContractParty` entities via `AddParty()`.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## Entity

* **ContractParty** — Represents a party to the contract with a role assignment.

## State Model

```
Draft ──Activate()──> Active ──Suspend()──> Suspended ──Terminate()──> Terminated
                      Active ──Terminate()──> Terminated
```

## Value Objects

* **ContractId** — Deterministic identifier (validated non-empty Guid).
* **ContractStatus** — Enum: `Draft`, `Active`, `Suspended`, `Terminated`.

## Events

* **ContractCreatedEvent** — Raised when a new contract is created (status: Draft).
* **ContractActivatedEvent** — Raised when contract is activated.
* **ContractSuspendedEvent** — Raised when contract is suspended.
* **ContractTerminatedEvent** — Raised when contract is terminated.

## Invariants

* ContractId must not be null/default.
* ContractStatus must be a defined enum value.
* Activation requires at least one party.
* State transitions enforced by specifications.

## Specifications

* **CanActivateSpecification** — Only Draft contracts can be activated.
* **CanSuspendSpecification** — Only Active contracts can be suspended.
* **CanTerminateSpecification** — Only Active or Suspended contracts can be terminated.

## Errors

* **MissingId** — ContractId is required.
* **AlreadyActive** — Cannot activate an already-active contract.
* **AlreadyTerminated** — Cannot act on a terminated contract.
* **InvalidStateTransition** — Generic guard for illegal status transitions.
* **PartyRequired** — Contract must have at least one party before activation.

## WHEN-NEEDED folders

- no `service/` — aggregate has no cross-aggregate coordination logic.

## Status

**S4 — Invariants + Specifications Complete**
