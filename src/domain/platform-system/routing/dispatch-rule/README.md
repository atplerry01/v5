# platform-system / routing / dispatch-rule

**Classification:** platform-system  
**Context:** routing  
**Domain:** dispatch-rule

## Purpose
A conditional rule that governs route eligibility during route-resolution. Dispatch
rules are evaluated in priority order to determine which route-definition wins for
a given message. Rules encode STRUCTURAL conditions only — never business logic,
never policy checks, never domain state reads.

## Aggregate: DispatchRuleAggregate

**Identity:** `DispatchRuleId` — SHA256 of (routeRef + conditionType + matchValue + priority)

**State:**
| Field | Type | Description |
|---|---|---|
| `DispatchRuleId` | value object (Guid) | Deterministic rule ID |
| `RuleName` | value object (string) | Human-readable rule label |
| `RouteRef` | value object (Guid) | RouteDefinitionId this rule applies to |
| `Condition` | DispatchCondition value object | Structural matching condition |
| `Priority` | value object (int ≥ 0) | Lower value = higher precedence |
| `Status` | enum | Active \| Inactive |

## Value Object: DispatchCondition
| Field | Type | Values |
|---|---|---|
| `ConditionType` | enum | MessageKindMatch \| SourceClassificationMatch \| DestinationContextMatch \| TransportHintMatch \| AlwaysMatch |
| `MatchValue` | string | Pattern to match against (empty string for AlwaysMatch) |

Condition types are STRUCTURAL — they match against message routing metadata only:
- `MessageKindMatch`: matches by MessageKind (Command, Event, Notification, Query)
- `SourceClassificationMatch`: matches by source DomainRoute classification segment
- `DestinationContextMatch`: matches by destination DomainRoute context segment
- `TransportHintMatch`: matches by TransportHint (Kafka, InProcess, Http, Grpc)
- `AlwaysMatch`: unconditional — used for default/fallback routes

## Events
| Event | Trigger |
|---|---|
| `DispatchRuleRegisteredEvent` | New dispatch rule registered and active |
| `DispatchRuleDeactivatedEvent` | Dispatch rule deactivated (no longer evaluated) |

## Invariants
- DispatchRuleId deterministic from (routeRef + conditionType + matchValue + priority)
- RuleName non-empty
- RouteRef references an Active RouteDefinition
- Condition ConditionType is explicit
- Priority is non-negative
- Priority is unique per RouteRef among Active rules of the same ConditionType
- No business-logic conditions — no policy checks, no domain state reads,
  no financial/governance/identity conditions permitted
- AlwaysMatch condition must have an empty MatchValue

## Errors
| Error | Condition |
|---|---|
| `DispatchRuleAlreadyRegisteredError` | Rule for same (routeRef, conditionType, matchValue, priority) already active |
| `InvalidDispatchConditionError` | ConditionType is unknown or MatchValue invalid for type |
| `PriorityConflictError` | Priority collides with existing active rule for same RouteRef + ConditionType |
| `BusinessLogicConditionForbiddenError` | Condition encodes business state (rejected at registration) |

## Commands
| Command | Description |
|---|---|
| `RegisterDispatchRule` | Register a new dispatch rule for a route-definition |
| `DeactivateDispatchRule` | Deactivate an active dispatch rule |

## Queries
| Query | Returns |
|---|---|
| `GetDispatchRule(dispatchRuleId)` | Single dispatch rule |
| `ListDispatchRules(routeRef?, status?)` | Filtered rule list |

## Projection Needs
`DispatchRuleView`: dispatchRuleId, ruleName, routeRef, condition, priority, status

## Kafka Topic
`whyce.platform.routing.dispatch-rule.events`

## Dependencies
- `core-system/identifier` — IIdGenerator for DispatchRuleId
- `routing/route-definition` — RouteRef target (cross-domain reference via ID only)
