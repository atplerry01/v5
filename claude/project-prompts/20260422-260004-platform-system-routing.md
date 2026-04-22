---
TITLE: platform-system / routing — E1→EX Canonical Batch
CLASSIFICATION: platform-system
CONTEXT: routing
DOMAIN GROUP: routing (3-level form — no physical domain-group folder segment)
DOMAINS:
  - route-definition
  - route-resolution
  - dispatch-rule
BATCH_DESCRIPTION: >
  Defines deterministic message delivery rules for the platform. The routing context
  owns the canonical model for declaring routes (route-definition), resolving which
  route applies for a given message (route-resolution), and the conditions that
  govern route selection (dispatch-rule). Routing must be deterministic, traceable,
  and free of business logic.
MIGRATION_NOTE: >
  The existing domain `route-descriptor` (src/domain/platform-system/routing/route-descriptor/)
  is a legacy scaffold that does not match the platform-system topic specification.
  The canonical 3 domains (route-definition, route-resolution, dispatch-rule) are the
  correct domains per the topic. route-descriptor is superseded; it should be
  deprecated/renamed in the routing context README and replaced by the 3 canonical domains.
SOURCE: claude/project-topics/v3/platform-system.md
TEMPLATE: claude/templates/pipelines/generic-prompt.md
---

# WBSM $3 MANDATORY SECTIONS

## TITLE
platform-system / routing — E1→EX Canonical Domain Batch

## CONTEXT
The `routing` context of platform-system owns the canonical model for determining
how messages (commands and events) are directed to their destinations:
- route-definition: the named route contract (source → destination + transport hints)
- route-resolution: the act of determining which route-definition applies for a message
- dispatch-rule: the conditional rules that govern route selection priority and eligibility

Current state:
- route-descriptor: ✓ scaffolded (WRONG NAME — legacy domain, does not match topic)
- route-definition: MISSING — canonical domain, must be created
- route-resolution: MISSING — canonical domain, must be created
- dispatch-rule: MISSING — canonical domain, must be created

Migration: route-descriptor is superseded by route-definition. Update routing/README.md
to reflect the canonical 3-domain model and mark route-descriptor as LEGACY/SUPERSEDED.

Physical path: `src/domain/platform-system/routing/{domain}/`
Form: 3-level (no domain-group segment)

Routing invariants (per platform-system topic §7):
- Routing MUST be deterministic
- Routing MUST be traceable
- Routing MUST NOT depend on business logic
- Routing MUST support idempotency
- Routing MUST NOT mutate message content

## OBJECTIVE
1. Create 3 canonical routing domains: route-definition, route-resolution, dispatch-rule.
2. Update routing/README.md to list canonical domains and mark route-descriptor as legacy.
3. Deliver full E1→EX for the routing context.

## CONSTRAINTS
- All routing must be deterministic — same input → same route, always
- No business logic in routing — no conditionals on domain state or business rules
- Routing depends only on core-system (identifier) and structural metadata
- No Guid.NewGuid(), no DateTime.UtcNow — IIdGenerator + IClock
- Domain layer: zero external dependencies
- Route resolution must be traceable — every resolution produces an audit trail

## EXECUTION STEPS

### STAGE E1 — DOMAIN MODEL

#### route-definition
Purpose: A named route that declares the source DomainRoute, destination DomainRoute,
transport hint, and priority. This is the CONTRACT for message routing.

Replaces: route-descriptor (legacy) — same concept, canonical name from topic.

Aggregate: RouteDefinitionAggregate
- RouteDefinitionId (value object, SHA256 of source+destination+transportHint)
- RouteName (value object, non-empty string — human-readable label)
- SourceRoute (DomainRoute value object — three-tuple: classification, context, domain)
- DestinationRoute (DomainRoute value object)
- TransportHint (value object enum: Kafka | InProcess | Http | Grpc)
- Priority (value object, int ≥ 0 — lower value = higher priority)
- Status: Active | Inactive | Deprecated
Events:
- RouteDefinitionRegisteredEvent
- RouteDefinitionDeactivatedEvent
- RouteDefinitionDeprecatedEvent
Invariants:
- RouteDefinitionId is deterministic from source+destination+transportHint
- RouteName non-empty
- SourceRoute and DestinationRoute are valid DomainRoute three-tuples
- Priority is non-negative
- Deprecated routes cannot be reactivated
- A route may not route to itself (source ≠ destination)
Errors:
- RouteDefinitionAlreadyRegisteredError
- SelfRoutingError (source equals destination)
- InvalidDomainRouteError

#### route-resolution
Purpose: The structural model for resolving which route-definition applies for a given
message source and type. Captures the resolution input, the resolved route, and the
resolution strategy used.

Aggregate: RouteResolutionAggregate
- ResolutionId (value object, SHA256 of sourceRoute+messageType+resolvedAt)
- SourceRoute (DomainRoute value object — message source)
- MessageType (value object, string — command type or event type)
- ResolvedRouteRef (references RouteDefinitionId — the winning route)
- ResolutionStrategy (value object enum: ExactMatch | PrefixMatch | DefaultRoute)
- ResolvedAt (IClock value object)
- DispatchRulesEvaluated (collection of DispatchRuleRef — audit trail)
Events:
- RouteResolvedEvent
- RouteResolutionFailedEvent
Invariants:
- ResolutionId deterministic
- SourceRoute non-empty
- MessageType non-empty
- ResolvedRouteRef non-null on success
- DispatchRulesEvaluated non-empty (at least one rule considered)
- ResolutionStrategy explicit
Errors:
- RouteResolutionFailedError (no matching route)
- AmbiguousRouteResolutionError (multiple equal-priority routes)

#### dispatch-rule
Purpose: A conditional rule that governs route eligibility for route-resolution.
Dispatch rules are evaluated in priority order to determine which route-definition
wins for a given message. Rules encode STRUCTURAL conditions only — no business logic.

Aggregate: DispatchRuleAggregate
- DispatchRuleId (value object, SHA256 of routeRef+condition+priority)
- RuleName (value object, non-empty string)
- RouteRef (references RouteDefinitionId — route this rule applies to)
- Condition (value object, DispatchCondition — see below)
- Priority (value object, int ≥ 0 — lower = higher precedence)
- Status: Active | Inactive
Events:
- DispatchRuleRegisteredEvent
- DispatchRuleDeactivatedEvent

Value Object: DispatchCondition
- ConditionType (enum: MessageKindMatch | SourceClassificationMatch |
    DestinationContextMatch | TransportHintMatch | AlwaysMatch)
- MatchValue (string — pattern to match; empty for AlwaysMatch)

Invariants:
- DispatchRuleId deterministic
- RuleName non-empty
- RouteRef references an Active RouteDefinition
- Condition is structurally valid
- Priority unique per RouteRef for Active rules
- No business-logic conditions (no policy checks, no domain state reads)
Errors:
- DispatchRuleAlreadyRegisteredError
- InvalidDispatchConditionError
- PriorityConflictError

### STAGE E2 — COMMAND LAYER
route-definition: RegisterRouteDefinition, DeactivateRouteDefinition, DeprecateRouteDefinition
route-resolution: ResolveRoute
dispatch-rule: RegisterDispatchRule, DeactivateDispatchRule

### STAGE E3 — QUERY LAYER
GetRouteDefinition(routeDefinitionId), ListRouteDefinitions(status?, source?, destination?)
GetRouteResolution(resolutionId), GetRouteResolutionBySource(sourceRoute, messageType)
GetDispatchRule(dispatchRuleId), ListDispatchRules(routeRef?, status?)

### STAGE E4 — T2E ENGINE HANDLERS
Standard T2E. RouteResolution handler must read Active RouteDefinitions + Active DispatchRules
via IEventStore (or read-model) to determine winning route. Resolution is DETERMINISTIC:
given the same set of rules, the same messageType and sourceRoute always produce the same result.

### STAGE E5 — POLICY INTEGRATION
No policy ownership. Routing has no policy evaluation.
Policy action names (informational): platform.routing.route.register, platform.routing.dispatch-rule.register

### STAGE E6 — EVENT FABRIC
Topics:
- whyce.platform.routing.route-definition.events
- whyce.platform.routing.route-resolution.events
- whyce.platform.routing.dispatch-rule.events

### STAGE E7 — PROJECTIONS
- RouteDefinitionView (routeDefinitionId, routeName, source, destination, transportHint, priority, status)
- RouteResolutionView (resolutionId, sourceRoute, messageType, resolvedRouteRef, strategy, resolvedAt)
- DispatchRuleView (dispatchRuleId, ruleName, routeRef, condition, priority, status)
- RoutingTableView (aggregate: messageType → resolvedRoute mapping for fast lookup)

### STAGE E8 — API LAYER
POST/GET /api/platform/routing/route-definitions
DELETE /api/platform/routing/route-definitions/{id}      (deactivate)
POST /api/platform/routing/resolve                        (resolve route for message)
GET /api/platform/routing/resolutions/{id}
POST/GET /api/platform/routing/dispatch-rules
DELETE /api/platform/routing/dispatch-rules/{id}

### STAGE E9 — WORKFLOW
No T1M workflow. All direct T2E.

### STAGE E10 — OBSERVABILITY
Metrics:
- whyce.platform.routing.route.registered.total
- whyce.platform.routing.resolution.success.total
- whyce.platform.routing.resolution.failure.total
- whyce.platform.routing.dispatch-rule.evaluated.total

### STAGE E11 — SECURITY
Service identity required for route registration. Routing resolution is internal.

### STAGE E12 — E2E VALIDATION
- Register route-definition + dispatch-rule → resolve route for messageType
- Verify determinism: same input produces same RouteResolutionId on re-run
- Verify priority ordering: higher-priority dispatch-rule wins when two rules match
- Verify failure path: no matching route → RouteResolutionFailedEvent
- Kafka: events on whyce.platform.routing.*.events topics

## OUTPUT FORMAT
Per domain: aggregate, value objects, events, errors, specifications, README.md
Also: update src/domain/platform-system/routing/README.md to:
  - List 3 canonical domains (route-definition, route-resolution, dispatch-rule)
  - Mark route-descriptor as LEGACY/SUPERSEDED

## VALIDATION CRITERIA
- [ ] route-definition, route-resolution, dispatch-rule domains created
- [ ] routing/README.md updated to reflect canonical 3 domains
- [ ] route-descriptor marked as LEGACY/SUPERSEDED in routing/README.md
- [ ] Routing determinism invariant documented in all resolution paths
- [ ] DispatchCondition uses structural conditions only (no business logic)
- [ ] AmbiguousRouteResolutionError defined for tie-break failures
- [ ] No Guid.NewGuid(), no DateTime.UtcNow
- [ ] Topics follow whyce.platform.routing.{domain}.events pattern
