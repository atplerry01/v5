# control-system / configuration

**Classification:** control-system  
**Context:** configuration

## Purpose
Owns the domain model for runtime configuration: how configuration entries are defined, what state they're in, what scope they apply to, and how resolved values are derived. Four sub-domains; no monolithic "configuration-entry" concept.

## Sub-domains
| Domain | Responsibility |
|---|---|
| `configuration-definition` | The canonical definition of a configuration key: name, type, default, description |
| `configuration-state` | The current value and lifecycle state of a configuration entry at runtime |
| `configuration-scope` | The scope declaration: which classification/context pair a configuration applies to |
| `configuration-resolution` | The resolved value record: what configuration was in effect at a given point |

## Does Not Own
- Policy definitions (→ system-policy)
- Feature flags or A/B testing (deferred — domain-specific)
- Infrastructure-layer configuration files

## Dependency Direction
- `configuration-state` depends on `configuration-definition` (by definition ID reference)
- `configuration-scope` depends on `configuration-definition` (by definition ID reference)
- `configuration-resolution` depends on `configuration-state` + `configuration-scope` (by ID references)

## Dependencies
- `core-system/identifier` — definition, state, scope, and resolution IDs
- `core-system/temporal` — effective periods for state and resolution records
