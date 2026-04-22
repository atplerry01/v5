# platform-system

**Classification:** Communication  
**Role:** Command, event, envelope, routing, and schema contracts

## Purpose
Owns the canonical communication contract layer: the structural shapes of commands, events,
transport-agnostic envelopes, routing rules, and schema definitions. Carries no business
semantics — only protocol structure, addressability, and schema contracts.

## Contexts
| Context | Responsibility |
|---|---|
| `command` | Command type definitions, envelopes, metadata, and routing contracts |
| `event` | Event type definitions, schemas, envelopes, metadata, and stream contracts |
| `envelope` | Transport-agnostic message wrapper: message-envelope, header, payload, metadata |
| `routing` | Deterministic route definitions, resolution logic, and dispatch rules |
| `schema` | Canonical schema definitions, contracts, versioning rules, and serialization formats |

## Does Not Own
- Business logic or policy evaluation (→ control-system)
- Domain-specific naming or aggregates (deferred to consuming systems)
- Authorization or access decisions (→ control-system)
- Temporal primitives (→ core-system)

## Invariants (platform-system triad rules)
- May depend on `core-system` only (identifier + temporal primitives)
- Must remain business-agnostic and policy-agnostic
- Must not encode domain-specific semantics
- Must not encode authorization or governance logic

## Dependencies
- `core-system` — identifier and temporal primitives only
