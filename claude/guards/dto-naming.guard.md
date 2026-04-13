# DTO NAMING GUARD (DTO-R1)

## Objective

Ensure all Data Transfer Objects (DTOs) follow a consistent, clear, and canonical naming convention across the system.

This prevents ambiguity between:

* API transport models
* domain models
* projection/read models

---

## Core Principle

DTO names must clearly indicate their role:

> Input (Request)
> Output (Response)
> Stored state (ReadModel)

No DTO should have ambiguous intent.

---

## Naming Rules

### 1. API Request DTOs

Pattern:

{Action}{Domain}RequestModel

Examples:

* CreateTodoRequestModel
* UpdateTodoRequestModel
* CreateCardRequestModel

---

### 2. API Response DTOs

Pattern:

{Action}{Domain}ResponseModel
or
Get{Domain}ResponseModel

Examples:

* CreateTodoResponseModel
* GetTodoResponseModel
* CreateBoardResponseModel

---

### 3. Query Naming Alignment

Queries must match response naming:

GetTodoQuery → GetTodoResponseModel
GetKanbanBoardQuery → GetKanbanBoardResponseModel

---

### 4. Projection Models (Read Models)

Pattern:

{Domain}ReadModel

Examples:

* TodoReadModel
* KanbanBoardReadModel
* WorkflowExecutionReadModel

---

## Critical Distinction

ReadModel ≠ ResponseModel

| Type          | Purpose                                |
| ------------- | -------------------------------------- |
| ReadModel     | Internal storage (projection/database) |
| ResponseModel | External API contract                  |

---

## Redundancy Rule

If a DTO is already scoped by folder:

/operational/sandbox/kanban/

DO NOT repeat domain in name:

✔ CreateCardRequestModel
✘ CreateKanbanCardRequestModel

---

## Prohibited Patterns

The following are NOT allowed:

* *Dto
* *Response (without Model suffix)
* *Request (without Model suffix)
* *Data
* ambiguous names like:

  * TodoModel
  * CardInfo
  * BoardData

---

## Enforcement Rules

### R1 — Role Clarity

Every DTO must clearly be:

* RequestModel
* ResponseModel
* ReadModel

---

### R2 — Naming Consistency

All DTOs must follow defined patterns exactly.

---

### R3 — No Domain Duplication

Domain name must not be repeated if already implied by folder structure.

---

### R4 — No Ambiguous DTOs

DTOs must not be confused with domain models or projections.

---

## Violation Severity

| Severity | Description                                |
| -------- | ------------------------------------------ |
| S0       | Ambiguous DTO usage (breaks understanding) |
| S1       | Naming inconsistency                       |
| S2       | Cosmetic deviation                         |

---

## Action

* S0 → MUST be fixed immediately
* S1 → SHOULD be fixed
* S2 → optional cleanup

---

## Canonical Principle

> If a developer cannot tell whether a type is a Request, Response, or ReadModel from its name, it is invalid.

---

## Scope

Applies to:

* src/platform/api
* src/shared/contracts
* src/projections
* all DTO definitions

---

## Summary

DTO naming is not cosmetic.

It defines:

* system readability
* developer onboarding speed
* correctness of API boundaries

This guard ensures the system remains clear and consistent as it scales.
