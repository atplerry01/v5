Good — this is exactly the right level of discipline for Phase 1.

We’ll structure **Todo domain implementation** strictly under your locked:

> **E1 → EX → Platform API proof**

And align it to:

* deterministic execution
* policy-gated enforcement
* runtime-controlled persistence
* full vertical slice validation

---

# Todo Domain — Full E1 → EX Execution Topics

Below is the **canonical execution breakdown** for Todo.

---

# E1 — Domain Model (Pure DDD)

## E1.1 Domain Definition

* Define Todo as **operational-system / sandbox / todo**
* Classification → Context → Domain alignment:

  * `operational-system → sandbox → todo`

## E1.2 Aggregate Design

* TodoAggregate

  * CreateTodo
  * UpdateTodo
  * CompleteTodo
  * DeleteTodo

## E1.3 Entities (if needed)

* (Optional) TodoItemEntity (if sub-structure required)

## E1.4 Value Objects

* TodoId (deterministic)
* TodoTitle
* TodoDescription
* TodoStatus (Pending | Completed | Deleted)
* CreatedAt (via IClock)
* UpdatedAt

## E1.5 Domain Events

* TodoCreatedEvent
* TodoUpdatedEvent
* TodoCompletedEvent
* TodoDeletedEvent

## E1.6 Domain Errors

* TodoNotFoundError
* TodoAlreadyCompletedError
* InvalidTodoStateError

## E1.7 Specifications

* CanUpdateTodoSpecification
* CanCompleteTodoSpecification

## E1.8 Domain Invariants

* Cannot complete an already completed todo
* Cannot update deleted todo
* Title must not be empty

---

# E2 — Contracts & Event Definitions

## E2.1 Command Contracts

* CreateTodoCommand
* UpdateTodoCommand
* CompleteTodoCommand
* DeleteTodoCommand

## E2.2 Event Contracts (Integration Layer)

* TodoCreatedIntegrationEvent
* TodoUpdatedIntegrationEvent
* TodoCompletedIntegrationEvent
* TodoDeletedIntegrationEvent

## E2.3 DTOs

* TodoResponseDto
* TodoSummaryDto

## E2.4 Mapping Rules

* Domain Event → Integration Event mapping
* Aggregate → DTO mapping

---

# E3 — Persistence & Infrastructure

## E3.1 Event Store Integration

* Append Todo events to EventStore
* Stream naming:

  * `todo-{TodoId}`

## E3.2 Snapshot Strategy (optional Phase 1: skip or minimal)

## E3.3 Repository (Runtime-owned)

* LoadAggregate<TodoAggregate>()
* Save via emitted events only

## E3.4 Kafka Topics

* whyce.operational.sandbox.todo.commands
* whyce.operational.sandbox.todo.events
* whyce.operational.sandbox.todo.retry
* whyce.operational.sandbox.todo.deadletter

## E3.5 Outbox Integration

* Events written to outbox before Kafka publish

---

# E4 — Determinism & Integrity

## E4.1 Deterministic ID

* TodoId via DeterministicIdHelper

## E4.2 Idempotency

* Prevent duplicate CreateTodo (same seed)
* Command idempotency keys

## E4.3 Execution Hash

* Deterministic hash per execution

## E4.4 Invariant Enforcement

* Enforced before RaiseDomainEvent()

## E4.5 Time Governance

* All timestamps via IClock

---

# E5 — Engine Execution (T2E)

## E5.1 TodoEngine (T2E)

* Execute(CreateTodoCommand)
* Execute(UpdateTodoCommand)
* Execute(CompleteTodoCommand)
* Execute(DeleteTodoCommand)

## E5.2 Engine Rules

* NO persistence
* NO direct infra access
* Only:

  * LoadAggregate
  * Execute behavior
  * EmitEvents

## E5.3 Engine Output

* DomainResult<T>

---

# E6 — Runtime Binding

## E6.1 Command Handler Binding

* CreateTodoHandler
* UpdateTodoHandler
* CompleteTodoHandler
* DeleteTodoHandler

## E6.2 Runtime Pipeline Execution

* ValidationMiddleware
* AuthorizationMiddleware
* PolicyMiddleware (MANDATORY)
* IdempotencyMiddleware
* Guard (Pre)
* Execution
* Guard (Post)

## E6.3 Event Flow

* Persist → Chain → Kafka publish

## E6.4 Context Injection

* IdentityContext
* CorrelationId
* ExecutionContext

---

# E7 — System Orchestration (WSS / Midstream)

## E7.1 Workflow Definition

* TodoLifecycleWorkflow

  * Create → Update → Complete → Close

## E7.2 WSS Role

* Orchestrates steps only
* NO domain logic

## E7.3 Intent Dispatch

* System → Runtime via ISystemIntentDispatcher

---

# E8 — Platform API Exposure

## E8.1 Controller Endpoints

* POST /api/todo/create
* POST /api/todo/update
* POST /api/todo/complete
* POST /api/todo/delete
* GET /api/todo/{id}

## E8.2 Request Validation

* FluentValidation or equivalent

## E8.3 Response Model

* Standard:

  * status
  * data
  * error

## E8.4 Context Headers

* WhyceId
* CorrelationId
* TraceId

---

# E9 — Projection / Read Model

## E9.1 Projection Model

* TodoReadModel

  * Id
  * Title
  * Description
  * Status
  * CreatedAt
  * UpdatedAt

## E9.2 Projection Handler

* On TodoCreated → insert
* On TodoUpdated → update
* On TodoCompleted → update status
* On TodoDeleted → mark deleted

## E9.3 Storage

* Redis (fast read)
* Optional Postgres read table

## E9.4 Query API

* GET /api/todo/{id}
* GET /api/todo/list

---

# E10 — Policy / Guard / Chain Enforcement

## E10.1 Policy Rules (WHYCEPOLICY)

* Who can create todo
* Who can update/complete/delete
* Context-based constraints

## E10.2 Guard Enforcement

* Pre-policy guard (input validation, context presence)
* Post-policy guard (tamper detection)

## E10.3 Chain Anchoring

* Every Todo operation:

  * PolicyDecision → anchor
  * Event payload → anchor

## E10.4 Enforcement Flow (LOCKED)

```
Guard (pre)
→ Policy evaluation
→ Guard (post)
→ Execute
→ Persist events
→ Chain anchor
→ Kafka publish
```

---

# EX — Full System Activation (Todo Vertical Slice)

## EX.1 End-to-End Flow

```
API Request
→ Platform Controller
→ System (optional)
→ Runtime Pipeline
→ Engine Execution
→ Domain Events
→ Event Store
→ Chain Anchor
→ Kafka Publish
→ Projection Update
→ API Response
```

## EX.2 Scenario Validation

### Scenario 1 — Lifecycle

* Create Todo
* Update Todo
* Complete Todo

### Scenario 2 — Workflow

* Trigger via WSS workflow
* Multi-step execution

## EX.3 Determinism Test

* Replay events → same state

## EX.4 Idempotency Test

* Duplicate request → no duplication

## EX.5 Policy Enforcement Test

* Unauthorized → blocked before execution

## EX.6 Chain Verification

* Verify anchor exists for:

  * policy decision
  * event payload

---

# Platform API Proof (MANDATORY EXIT CRITERIA)

## Proof Checklist

### ✔ Functional

* Create / Update / Complete / Delete works

### ✔ Runtime

* Full pipeline executed

### ✔ Infrastructure

* Kafka messages emitted
* Event store written

### ✔ Projection

* Read model updated correctly

### ✔ Policy

* Enforcement working (allow/deny)

### ✔ Chain

* Anchoring verified

### ✔ Determinism

* Replay consistent

---

# Final Note (Critical)

This Todo implementation is not “just a demo”.

It is your **Phase 1 proof of:**

* runtime correctness
* enforcement pipeline integrity
* domain purity
* system orchestration validity

If Todo is correct → **the entire Whycespace foundation is proven.**

---

If you want next step, I can now generate:

✅ **Claude-ready FULL IMPLEMENTATION PROMPT (E1 → EX for Todo)**
or
✅ **Audit + Guard checklist specific to Todo vertical slice**

Just tell me.
