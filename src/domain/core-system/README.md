# core-system

**Classification:** Language  
**Role:** Minimal universal primitives — immutable, behaviorless, zero dependencies

## Purpose
Owns only the foundational language of the system. Every concept here is an immutable value object or pure structural type with no lifecycle, no state transitions, and no behavior beyond validation. All other classifications depend downward into this layer; nothing here depends upward.

## Contexts
| Context | Sub-domains | Role |
|---|---|---|
| `temporal` | time-window, time-point, time-range, effective-period | Bounded and point-in-time temporal expressions |
| `ordering` | sequence, ordering-key, ordering-rule | Monotonic position, stable ordering primitives, and deterministic ordering criteria |
| `identifier` | global-identifier, entity-reference, correlation-id, causation-id | Deterministic reference types |

## Hard Rules
- NO services
- NO aggregates with lifecycle
- NO state transitions
- NO policies
- NO orchestration
- NO behavior beyond validation
- ALL value types are immutable once constructed
- NO `Guid.NewGuid()`, `DateTime.Now`, `Random` — generation belongs to engine layer

## Dependencies
- **None** — zero external dependencies, by design and by rule
