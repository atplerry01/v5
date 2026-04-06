# Enforcement Domain

## 📌 Purpose

This subdomain represents the **policy enforcement and violation tracking** within the Policy domain.

It is responsible for:
- Policy enforcement execution
- Policy violation recording
- Violation severity classification

---

## 🧱 Structure Overview

This subdomain follows strict WBSM v3 domain modeling rules:

├── aggregates/        # State + invariants (ONLY place for mutation)
├── entities/          # Supporting domain objects with identity
├── value-objects/     # Immutable domain primitives
├── events/            # Domain events (state changes)
├── services/          # Domain logic that doesn't fit aggregates
├── errors/            # Domain-specific error definitions
└── specifications/    # Business rules / validation logic

---

## 🧠 Component Responsibilities

### 1. Aggregates
- The ONLY place where state changes occur
- Enforce business invariants
- Must be consistency boundaries

✅ Use when:
- Managing lifecycle
- Enforcing rules
- Coordinating entities

❌ DO NOT:
- Call external systems
- Contain infrastructure logic

---

### 2. Entities
- Have identity
- Belong to an aggregate
- Support aggregate behavior

✅ Use when:
- Object has identity but is not root

---

### 3. Value Objects
- Immutable
- Equality by value
- Encapsulate validation

✅ Use when:
- Representing concepts like Money, Status, Type

❌ DO NOT:
- Include identity
- Be mutable

---

### 4. Events
- Represent something that HAS happened
- Immutable
- Must be emitted on every state change

✅ Naming:
- Past tense (e.g. CapitalContributedEvent)

✅ Must include:
- AggregateId
- Timestamp
- Relevant data

---

### 5. Services
- Stateless domain logic
- Used when logic doesn't belong in aggregate/entity

✅ Use when:
- Cross-entity/domain logic
- Complex calculations

---

### 6. Errors
- Centralized domain errors

✅ Must:
- Be explicit
- Be descriptive
- Be deterministic

Example:
- InsufficientBalanceError
- InvalidStateTransitionError

---

### 7. Specifications
- Encapsulate business rules
- Reusable validation logic

✅ Use when:
- Rule is reusable
- Rule is complex

Example:
- SufficientBalanceSpec
- CapitalAvailabilitySpec

---

## 🔁 Relationships (IMPORTANT)

- Aggregates USE Entities and ValueObjects
- Aggregates EMIT Events
- Services MAY use Aggregates/Entities
- Specifications VALIDATE Aggregates/Entities
- Errors are used across all layers

---

## ➕ Adding New Files (STRICT RULES)

### ✅ Add a new Aggregate ONLY if:
- It represents a true consistency boundary
- It owns its lifecycle

---

### ✅ Add a new Entity ONLY if:
- It has identity
- It belongs to an aggregate

---

### ✅ Add a new Value Object ONLY if:
- It is immutable
- It represents a domain concept

---

### ✅ Add a new Event ONLY if:
- A state change occurs
- It is meaningful to the system

---

### ✅ Add a new Service ONLY if:
- Logic cannot live inside aggregate/entity

---

### ✅ Add a new Specification ONLY if:
- Rule is reusable
- Rule is complex

---

### ❌ NEVER ADD:
- Infrastructure logic
- Database logic
- API logic
- Engine calls

---

## 🚫 Anti-Patterns (FORBIDDEN)

- ❌ Business logic in services instead of aggregates
- ❌ Mutable value objects
- ❌ Missing domain events
- ❌ Cross-aggregate mutation
- ❌ God aggregates

---

## 🔒 WBSM v3 Compliance

This subdomain is:

- ✅ Event-sourced ready
- ✅ Deterministic
- ✅ Policy-enforceable
- ✅ Engine-aligned (T2E)
