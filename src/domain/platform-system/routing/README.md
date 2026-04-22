# platform-system / routing

**Classification:** platform-system  
**Context:** routing

## Purpose
Defines deterministic message delivery rules. The routing context owns the canonical
structural model for declaring named routes (route-definition), resolving which route
applies for a given message (route-resolution), and the conditions that govern route
selection (dispatch-rule).

Routing invariants (per platform-system topic §7):
- Routing MUST be deterministic — same input always produces same route
- Routing MUST be traceable — every resolution produces an audit record
- Routing MUST NOT depend on business logic — structural conditions only
- Routing MUST support idempotency
- Routing MUST NOT mutate message content

## Domains (Canonical)
| Domain | Responsibility |
|---|---|
| `route-definition` | Named route contract: source DomainRoute → destination DomainRoute + transport hint + priority |
| `route-resolution` | Structural model for resolving which route-definition applies for a message |
| `dispatch-rule` | Conditional rules governing route selection priority and eligibility |

## Legacy Domain
| Domain | Status |
|---|---|
| `route-descriptor` | **LEGACY / SUPERSEDED** — replaced by `route-definition`. Do not extend. |

## Does Not Own
- Command-specific routing rules (→ command/command-routing)
- Event stream subscriptions (→ event/event-stream)
- Policy governing routing (→ control-system/system-policy)
- Transport execution (→ infrastructure/event-fabric)
- Business logic conditions (→ no business logic permitted in routing)

## Outputs
- `RouteDefinitionRegisteredEvent`, `RouteDefinitionDeactivatedEvent`, `RouteDefinitionDeprecatedEvent`
- `RouteResolvedEvent`, `RouteResolutionFailedEvent`
- `DispatchRuleRegisteredEvent`, `DispatchRuleDeactivatedEvent`

## Invariants
- All IDs deterministic: SHA256-derived via IIdGenerator
- A route may not route to itself (source ≠ destination)
- One active route per (source, destination, transportHint) combination
- Dispatch rules use STRUCTURAL conditions only — no domain state reads, no policy checks
- Route resolution is deterministic: same (sourceRoute, messageType, activeRules) → same result

## Dependencies
- `core-system/identifier` — deterministic IDs via IIdGenerator
- `core-system/temporal` — IClock for ResolvedAt timestamps
