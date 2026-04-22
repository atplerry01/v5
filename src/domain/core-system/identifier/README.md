# core-system / identifier

**Classification:** core-system  
**Context:** identifier

## Purpose
Identifier primitives: deterministic, immutable reference types used across all classifications. Every identifier in the system is structurally derived from one of these four primitives.

## Sub-domains
| Domain | Responsibility |
|---|---|
| `global-identifier` | System-wide unique, deterministic identifier (SHA256-based) |
| `entity-reference` | Typed reference to a specific domain entity by its global identifier |
| `correlation-id` | Cross-boundary correlation key linking related commands and events |
| `causation-id` | Causal link declaring which event or command caused the current message |

## Hard Rules
- NO `Guid.NewGuid()` — all IDs are deterministic
- NO random or time-seeded generation in the domain layer
- Generation belongs to `IIdGenerator` in the engine layer

## Invariants
- All identifier values are immutable once constructed
- All are SHA256-derived (64 lowercase hex characters) unless structurally narrowed

## Dependencies
- None
