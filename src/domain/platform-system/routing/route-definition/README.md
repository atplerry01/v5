# platform-system / routing / route-definition

**Classification:** platform-system  
**Context:** routing  
**Domain:** route-definition

## Purpose
A named route that declares the source DomainRoute, destination DomainRoute, transport
hint, and priority. The route-definition is the CONTRACT for message routing — it states
where messages of a given source go, via which transport, and at what priority.

Supersedes the legacy `route-descriptor` domain.

## Aggregate: RouteDefinitionAggregate

**Identity:** `RouteDefinitionId` — SHA256 of (source + destination + transportHint)

**State:**
| Field | Type | Description |
|---|---|---|
| `RouteDefinitionId` | value object (Guid) | Deterministic route ID |
| `RouteName` | value object (string) | Human-readable label for the route |
| `SourceRoute` | DomainRoute value object | Three-tuple (classification, context, domain) |
| `DestinationRoute` | DomainRoute value object | Target DomainRoute three-tuple |
| `TransportHint` | enum value object | Kafka \| InProcess \| Http \| Grpc |
| `Priority` | value object (int ≥ 0) | Lower value = higher priority |
| `Status` | enum | Active \| Inactive \| Deprecated |

## Events
| Event | Trigger |
|---|---|
| `RouteDefinitionRegisteredEvent` | New route definition registered and active |
| `RouteDefinitionDeactivatedEvent` | Route definition deactivated (no longer resolved) |
| `RouteDefinitionDeprecatedEvent` | Route definition deprecated (cannot be reactivated) |

## Invariants
- RouteDefinitionId deterministic from (source + destination + transportHint)
- RouteName non-empty
- SourceRoute and DestinationRoute are valid DomainRoute three-tuples
- Source MUST NOT equal Destination (self-routing forbidden)
- Priority is non-negative
- Deprecated routes cannot be reactivated (compensation only — new route)
- Only one Active route may exist per (source, destination, transportHint) triplet

## Errors
| Error | Condition |
|---|---|
| `RouteDefinitionAlreadyRegisteredError` | Route for same (source, destination, transportHint) already active |
| `SelfRoutingError` | SourceRoute equals DestinationRoute |
| `InvalidDomainRouteError` | SourceRoute or DestinationRoute fails DomainRoute validation |
| `RouteDefinitionDeprecatedError` | Activation attempted on deprecated route |

## Commands
| Command | Description |
|---|---|
| `RegisterRouteDefinition` | Register a new route definition |
| `DeactivateRouteDefinition` | Deactivate an active route (stop resolving) |
| `DeprecateRouteDefinition` | Permanently deprecate a route |

## Queries
| Query | Returns |
|---|---|
| `GetRouteDefinition(routeDefinitionId)` | Single route definition |
| `ListRouteDefinitions(status?, source?, destination?)` | Filtered route list |

## Projection Needs
`RouteDefinitionView`: routeDefinitionId, routeName, sourceRoute, destinationRoute, transportHint, priority, status

## Kafka Topic
`whyce.platform.routing.route-definition.events`

## Dependencies
- `core-system/identifier` — IIdGenerator for RouteDefinitionId
