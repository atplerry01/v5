# control-system / system-policy

**Classification:** control-system  
**Context:** system-policy

## Purpose
Owns the authoritative definition, evaluation decision, and enforcement record of system-wide policies. Policy is the language by which the system declares what is permitted.

## Domains
| Domain | Responsibility |
|---|---|
| `policy-definition` | The canonical form of a policy: scope, conditions, effects |
| `policy-decision` | The evaluated output of policy against a subject and action |
| `policy-enforcement` | The enforcement record binding a decision to a system effect |

## Does Not Own
- Access control subjects or roles (→ access-control context)
- Audit trails (→ audit context)
- Domain-specific policy variants (deferred to Phase 2.5)

## Inputs
- Policy subject (actor, resource, action, context)
- Policy constraint expressions

## Outputs
- `PolicyDefinedEvent` — a policy has been registered
- `PolicyDecisionEvent` — a policy has been evaluated
- `PolicyEnforcedEvent` — enforcement has been applied

## Invariants
- A policy decision references exactly one policy definition
- An enforcement record references exactly one policy decision
- Policies are immutable once published; amendments create new versions

## Dependencies
- `core-system/identifier` — reference identifiers for policy IDs
- `core-system/temporal` — timestamps for decision and enforcement records
