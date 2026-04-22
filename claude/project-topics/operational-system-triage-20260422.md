# Operational-System BC Triage — 2026-04-22

## Classification Key
- **ACTIVE** — D2-complete: full event sourcing, typed VOs, rich invariants, ready for engine consumption
- **STUB** — D1 placeholder: aggregate shell exists, no identity/events/state, requires D2 promotion
- **DUPLICATE** — Structurally redundant; must be resolved (merged, removed, or renamed) before D2 promotion
- **PLANNED** — Not yet implemented; scoped for a future phase

---

## Context Map

### `sandbox/` context group

| Domain | Status | Notes |
|--------|--------|-------|
| `sandbox/kanban` | **ACTIVE** | `KanbanAggregate` — full lifecycle, 7 events, composite entity children (`KanbanList`, `KanbanCard`), full `EnsureInvariants`. Factory: `Create(KanbanBoardId, name)`. Well-typed VOs (`KanbanCardTitle`, `DocumentRef`, `KanbanPosition`, `KanbanPriority`). Namespace: `OperationalSystem.Sandbox.Kanban`. |
| `sandbox/todo` | **ACTIVE** | `TodoAggregate` — 3 events (`Created`, `TitleRevised`, `Completed`), guard-based invariants, completed flag. Factory: `Create(TodoId, title)`. Namespace: `OperationalSystem.Sandbox.Todo`. |

---

### `incident-response/` context group

| Domain | Status | Notes |
|--------|--------|-------|
| `incident-response/incident` | **ACTIVE** | `IncidentAggregate` — full 4-step lifecycle (`Reported → Investigating → Resolved → Closed`), spec-based state transitions, 4 events, typed `IncidentDescriptor` and `IncidentId`. `EnsureInvariants` guards identity + descriptor + enum validity. Namespace: `OperationalSystem.IncidentResponse.Incident`. |

---

### `deployment/` context group

| Domain | Status | Notes |
|--------|--------|-------|
| `deployment/activation` | **STUB** | `ActivationAggregate` — empty shell: no identity property, no events, no state machine, `EnsureInvariants` is comment-only. `Create()` calls `ValidateBeforeChange()` + `EnsureInvariants()` but emits nothing. Requires full D2 promotion: identity VO, events, lifecycle. Namespace: `OperationalSystem.Deployment.Activation`. |
| `deployment/emergency` | **STUB** | `EmergencyAggregate` — same empty pattern as above. Semantics: likely a break-glass / runbook deployment triggered by incident escalation. Requires D2 promotion. Namespace: `OperationalSystem.Deployment.Emergency`. |
| `deployment/sandbox` | **STUB** | `SandboxAggregate` — empty shell. **Naming collision risk** with `sandbox/kanban` and `sandbox/todo` contexts (all use "sandbox"). Clarification needed: is this a deployment sandbox environment aggregate or the same concern as `sandbox/kanban`? Resolve naming before D2 promotion. Namespace: `OperationalSystem.Deployment.Sandbox`. |

---

### `activation/` context group — DUPLICATE

| Domain | Status | Notes |
|--------|--------|-------|
| `activation/activation` | **DUPLICATE** | `ActivationAggregate` — identical stub body to `deployment/activation`. Same class name, same methods, different namespace (`OperationalSystem.Activation.Activation`). This appears to be a stale precursor to `deployment/activation`. **Action required before D2:** decide which path is canonical and delete the other. The double-nesting `activation/activation` also violates DS-R3 (classification/context/domain triple). |

---

### `incident/` context group — DUPLICATE

| Domain | Status | Notes |
|--------|--------|-------|
| `incident/response` | **DUPLICATE** | `ResponseAggregate` — empty stub. Coexists with `incident-response/incident` (ACTIVE). Path `incident/response` suggests a _response_ domain within an _incident_ context, while `incident-response/incident` is an _incident_ domain within an _incident-response_ context. These are distinct concerns conceptually but both are incomplete and overlapping. **Action required:** determine if `incident/response` is a separate BC (incident resolution actions) or a dead branch of `incident-response/incident`. Delete or promote. |

---

## Summary Table

| BC Path | Aggregate | Status | Action |
|---------|-----------|--------|--------|
| `sandbox/kanban` | `KanbanAggregate` | ACTIVE | Unit tests needed (Phase 1 backlog) |
| `sandbox/todo` | `TodoAggregate` | ACTIVE | Unit tests needed (Phase 1 backlog) |
| `incident-response/incident` | `IncidentAggregate` | ACTIVE | Unit tests needed (Phase 1 backlog) |
| `deployment/activation` | `ActivationAggregate` | STUB | D2 promotion required |
| `deployment/emergency` | `EmergencyAggregate` | STUB | D2 promotion required |
| `deployment/sandbox` | `SandboxAggregate` | STUB | Naming clarification + D2 promotion |
| `activation/activation` | `ActivationAggregate` | DUPLICATE | Delete or merge into `deployment/activation` |
| `incident/response` | `ResponseAggregate` | DUPLICATE | Delete or promote as separate BC |
| `invariant/command-integrity` | `CommandMustProduceDomainEventPolicy` | POLICY | Not an aggregate; policy artifact — no triage action needed |

---

## D2 Promotion Prerequisites (for all STUB BCs)

Before any stub can be promoted to D2, each needs:

1. Typed identity VO (`{Name}Id` wrapping `Guid`)
2. Typed descriptor or input VOs
3. At least one domain event (`{Name}CreatedEvent` minimum)
4. `Apply(object)` switch populating all properties from events
5. Non-trivial `EnsureInvariants()` with at least identity + state validation
6. Factory method with deterministic identity (no `Guid.NewGuid()`)
7. Unit tests covering factory emission, initial state, stable identity, `LoadFromHistory`

---

## Unit Test Backlog (ACTIVE BCs)

Three ACTIVE operational-system BCs are missing unit tests (Phase 1 backlog):

- `sandbox/kanban` — `KanbanAggregateTests`: factory, CreateList, CreateCard, MoveCard, CompleteCard, invariant guards
- `sandbox/todo` — `TodoAggregateTests`: Create, ReviseTitle, Complete, invariant guards
- `incident-response/incident` — `IncidentAggregateTests`: Report, Investigate, Resolve, Close, invalid-transition guards

These should be written in Phase 1 alongside structural-system and content-system test completion.
