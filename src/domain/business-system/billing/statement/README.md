# Domain: Statement

## Classification

business-system

## Context

billing

## Domain Responsibility

Models periodic billing statements that aggregate multiple invoices or receivables into a single summary view. Tracks statement lifecycle from draft through issued and closed states. Closing a statement locks it from further modification.

## Aggregate

* **StatementAggregate** — Root aggregate representing a statement instance.
  * Private constructor; created via `Create(StatementId)` factory method.
  * State transitions via `IssueStatement()` and `CloseStatement()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Draft ──IssueStatement()──> Issued ──CloseStatement()──> Closed (terminal)
```

## Entities

* **StatementLine** — Aggregated financial entry (LineId, SourceReferenceId, Description, Amount). Must have at least one before issuing.

## Value Objects

* **StatementId** — Deterministic identifier (validated non-empty Guid).
* **StatementStatus** — Enum: `Draft`, `Issued`, `Closed`.

## Events

* **StatementCreatedEvent** — Raised when a new statement is created (status: Draft).
* **StatementIssuedEvent** — Raised when statement is issued.
* **StatementClosedEvent** — Raised when statement is closed and locked.

## Invariants

* StatementId must not be null/default.
* StatementStatus must be a defined enum value.
* Cannot issue an empty statement (enforced by IsNonEmptyStatementSpecification).
* Cannot add lines to a closed statement.
* Closing locks the statement (enforced by CanCloseStatementSpecification: only Issued allows close).

## Specifications

* **CanIssueStatementSpecification** — Validates that status is Draft before issuing.
* **CanCloseStatementSpecification** — Validates that status is Issued before closing.
* **IsNonEmptyStatementSpecification** — Validates that statement has at least one line.

## Errors

* **MissingId** — StatementId is required.
* **EmptyStatement** — Statement must contain at least one line before issuing.
* **AlreadyIssued** — Cannot issue an already-issued statement.
* **AlreadyClosed** — Cannot close an already-closed statement.
* **InvalidStateTransition** — Generic guard for illegal status transitions.

## Domain Services

* **StatementService** — Reserved for cross-aggregate coordination within statement context.

## Status

**S4 — Invariants + Specifications Complete**
