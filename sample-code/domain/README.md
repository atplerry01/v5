# Workflow Domain

## 📌 Purpose

The Workflow subdomain manages **long-lived process orchestration**.

It is responsible for:
- Defining workflows
- Managing workflow instances
- Executing steps and transitions
- Tracking state progression

---

## 🧠 Core Invariants

- Workflow must follow defined transitions
- State must be valid at all times
- Steps must execute in order
- Workflow must be resumable

---

## 🧱 Structure Overview

├── aggregates/        # WorkflowDefinition, WorkflowInstance
├── entities/          # Steps, transitions
├── value-objects/     # State, context
├── events/            # Workflow lifecycle events
├── services/          # Orchestration helpers
├── errors/            # Transition errors
└── specifications/    # Transition validation

---

## 🔁 Relationships

- Definition defines structure
- Instance tracks execution
- Steps drive progression

---

## ➕ Adding Rules

- New workflow → Aggregate
- New step type → Entity
- New transition rule → Specification

---

## 🚫 Forbidden

- Skipping transitions
- Invalid state mutation
- Non-event-driven transitions

---

## 🔒 WBSM v3 Compliance

- Drives T1M orchestration
- Fully event-sourced

---

## 📂 Shared Code Convention

This domain layer uses two distinct shared code patterns:

- **`shared/`** (root level) — Global shared kernel. Contains base primitives used across ALL classifications: `AggregateRoot`, `Entity`, `ValueObject`, `DomainEvent`, `DomainException`, `Result`, `Specification`, `Timestamp`, `Description`, `Label`. Lives at `src/domain/shared/`.

- **`_shared/`** (classification level) — Classification-scoped shared logic. Each classification (e.g. `economic/`, `identity/`) may contain a `_shared/` folder for value objects, services, or specifications shared only within that classification. These are NOT globally visible.

This distinction is intentional and MUST be maintained. Do not merge `_shared/` into `shared/` or vice versa.