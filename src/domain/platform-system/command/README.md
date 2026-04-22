# platform-system / command

**Classification:** platform-system  
**Context:** command

## Purpose
Owns the pure structural model of a command: its envelope (addressing + metadata), definition (registered command types), and routing (how a command is directed to its handler).

## Domains
| Domain | Responsibility |
|---|---|
| `command-definition` | Registered command type definitions with schema reference and handler address |
| `command-envelope` | The canonical envelope wrapping a command payload: ID, type, source, destination, correlation |
| `command-metadata` | Structured metadata attached to a command envelope: actor, trace IDs, policy context ref, trust score |
| `command-routing` | Routing rules binding a command type to a handler address deterministically |

## Does Not Own
- Command execution (→ engine layer)
- Command authorization / policy evaluation (→ control-system/system-policy + runtime pipeline)
- Business-specific command payloads (→ consuming domain systems)
- Command catalog / discovery registry (→ deferred; out of scope for protocol layer)

## Inputs
- Command type registration at bootstrap
- Command routing rule from configuration

## Outputs
- `CommandDefinitionRegisteredEvent`, `CommandDefinitionDeprecatedEvent`
- `CommandEnvelopeCreatedEvent`
- `CommandMetadataAttachedEvent`
- `CommandRoutingRuleRegisteredEvent`, `CommandRoutingRuleDeactivatedEvent`

## Invariants
- All IDs are deterministic: SHA256-derived via IIdGenerator
- Every command type maps to exactly one active routing rule at any time
- Envelope structure is stable across versions; payload is opaque
- CommandMetadata carries no policy evaluation logic — structural carrier only
- TrustScore on CommandMetadata must be in range [0, 100]

## Dependencies
- `core-system/identifier` — command and correlation IDs
- `core-system/temporal` — command issuance timestamp
