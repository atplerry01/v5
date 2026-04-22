# platform-system / routing / route-resolution

**Classification:** platform-system  
**Context:** routing  
**Domain:** route-resolution

## Purpose
Models the act of determining which route-definition applies for a given message source
and type. Captures the resolution input, the winning route-definition reference, the
strategy used, and the dispatch rules evaluated — providing a complete audit trail for
every routing decision.

Route resolution is deterministic: the same (sourceRoute, messageType, active rules)
always produces the same resolved route.

## Aggregate: RouteResolutionAggregate

**Identity:** `ResolutionId` — SHA256 of (sourceRoute + messageType + resolvedAt)

**State:**
| Field | Type | Description |
|---|---|---|
| `ResolutionId` | value object (Guid) | Deterministic resolution ID |
| `SourceRoute` | DomainRoute value object | Message source three-tuple |
| `MessageType` | value object (string) | Command type name or event type name |
| `ResolvedRouteRef` | value object (Guid, nullable) | RouteDefinitionId of the winning route (null on failure) |
| `ResolutionStrategy` | enum value object | ExactMatch \| PrefixMatch \| DefaultRoute |
| `ResolvedAt` | value object (DateTimeOffset) | IClock timestamp |
| `DispatchRulesEvaluated` | collection (Guid) | DispatchRuleIds considered during resolution |
| `Outcome` | enum | Resolved \| Failed \| Ambiguous |

## Events
| Event | Trigger |
|---|---|
| `RouteResolvedEvent` | Route successfully resolved to a single route-definition |
| `RouteResolutionFailedEvent` | No matching route found for source + messageType |

## Invariants
- ResolutionId deterministic from (sourceRoute + messageType + resolvedAt)
- SourceRoute non-empty
- MessageType non-empty
- ResolvedRouteRef non-null when Outcome = Resolved
- DispatchRulesEvaluated non-empty (at least one rule must have been evaluated)
- ResolutionStrategy explicit
- Determinism guarantee: given identical (sourceRoute, messageType, active rules),
  the winning route is always the same — no randomness, no ambient state

## Errors
| Error | Condition |
|---|---|
| `RouteResolutionFailedError` | No active route-definition matches the source + messageType |
| `AmbiguousRouteResolutionError` | Multiple dispatch-rules at equal priority both apply |

## Commands
| Command | Description |
|---|---|
| `ResolveRoute` | Execute route resolution for a (sourceRoute, messageType) pair |

## Queries
| Query | Returns |
|---|---|
| `GetRouteResolution(resolutionId)` | Single resolution record |
| `GetRouteResolutionBySource(sourceRoute, messageType)` | Most recent resolution for source+type |

## Projection Needs
`RouteResolutionView`: resolutionId, sourceRoute, messageType, resolvedRouteRef, strategy, outcome, resolvedAt

## Kafka Topic
`whyce.platform.routing.route-resolution.events`

## Dependencies
- `core-system/identifier` — IIdGenerator for ResolutionId
- `core-system/temporal` — IClock for ResolvedAt
- `routing/route-definition` — ResolvedRouteRef target (cross-domain reference via ID only)
- `routing/dispatch-rule` — DispatchRulesEvaluated (cross-domain reference via IDs only)
