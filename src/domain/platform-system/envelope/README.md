# platform-system / envelope

**Classification:** platform-system  
**Context:** envelope

## Purpose
Defines the transport-agnostic message wrapping structure shared across all communication
contracts. The envelope context is the GENERAL-PURPOSE layer that applies to any message
type — commands, events, notifications, or queries — regardless of the specific type-level
envelope defined in the command or event contexts.

The envelope context provides the structural vocabulary for composing messages:
the universal wrapper (message-envelope), the routing and tracing header (header),
the opaque payload contract (payload), and the cross-cutting metadata (metadata).

## Distinction from command-envelope and event-envelope
| Layer | Purpose |
|---|---|
| `command/command-envelope` | Type-specific envelope for command messages with command-specific fields |
| `event/event-envelope` | Type-specific envelope for event messages with event-specific fields |
| `envelope/message-envelope` | General-purpose, transport-agnostic wrapper for ANY message type |

## Domains
| Domain | Responsibility |
|---|---|
| `message-envelope` | Universal message wrapper composing header + payload + metadata |
| `header` | Routing, tracing, content-type, and identity token structure |
| `payload` | Opaque payload contract — type reference, encoding, schema reference, bytes |
| `metadata` | Cross-cutting message metadata: correlation, causation, version, issuedAt |

## Does Not Own
- Type-specific envelope fields (→ command/command-envelope, event/event-envelope)
- Policy evaluation or authorization (→ control-system + runtime pipeline)
- Transport implementation (→ infrastructure/event-fabric)
- Business meaning or domain-specific semantics (→ consuming domain systems)

## Outputs
- `MessageEnvelopeCreatedEvent`, `MessageEnvelopeDispatchedEvent`
- `MessageEnvelopeAcknowledgedEvent`, `MessageEnvelopeRejectedEvent`
- `HeaderSchemaRegisteredEvent`, `HeaderSchemaDeprecatedEvent`
- `PayloadSchemaRegisteredEvent`, `PayloadSchemaDeprecatedEvent`
- `MessageMetadataSchemaRegisteredEvent`

## Invariants
- All IDs are deterministic: SHA256-derived via IIdGenerator
- message-envelope composes header + payload + metadata — all three required
- CorrelationId and CausationId propagated intact through metadata value object
- Dispatched envelopes are terminal — status transitions: Created → Dispatched → Acknowledged | Rejected

## Dependencies
- `core-system/identifier` — deterministic IDs via IIdGenerator
- `core-system/temporal` — IssuedAt via IClock
