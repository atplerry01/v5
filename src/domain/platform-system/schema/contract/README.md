# platform-system / schema / contract

**Classification:** platform-system  
**Context:** schema  
**Domain:** contract

## Purpose
A contract binds a schema-definition to a publisher and one or more subscriber
constraints. Contracts are the enforcement mechanism — they ensure publishers emit
schema-conformant messages and subscribers declare their schema compatibility expectations.

A contract is the agreed-upon structural obligation between message producers and consumers.
It carries no business meaning — only protocol-level structural obligations.

## Aggregate: ContractAggregate

**Identity:** `ContractId` — SHA256 of (publisherRoute + schemaRef + schemaVersion)

**State:**
| Field | Type | Description |
|---|---|---|
| `ContractId` | value object (Guid) | Deterministic contract ID |
| `ContractName` | value object (string) | Human-readable contract label |
| `PublisherRoute` | DomainRoute value object | The system/context that publishes this schema |
| `SchemaRef` | value object (Guid) | Reference to schema-definition ID |
| `SchemaVersion` | int | Pinned published schema version |
| `SubscriberConstraints` | collection (SubscriberConstraint) | Subscriber compatibility obligations |
| `Status` | enum | Active \| Deprecated |

## Value Object: SubscriberConstraint
| Field | Type | Description |
|---|---|---|
| `SubscriberRoute` | DomainRoute | The consuming system/context |
| `MinSchemaVersion` | int | Minimum schema version the subscriber can accept |
| `RequiredCompatibilityMode` | CompatibilityMode enum | Backward \| Forward \| Full \| None |

## Events
| Event | Trigger |
|---|---|
| `ContractRegisteredEvent` | New contract registered |
| `ContractSubscriberAddedEvent` | Subscriber constraint added to existing contract |
| `ContractDeprecatedEvent` | Contract deprecated (no new messages expected) |

## Invariants
- ContractId deterministic from (publisherRoute + schemaRef + schemaVersion)
- PublisherRoute is a valid DomainRoute three-tuple
- SchemaRef must reference a Published schema-definition (not Draft or Deprecated)
- SchemaVersion matches the pinned published schema version
- Every SubscriberConstraint declares MinSchemaVersion ≥ 1
- Deprecated contracts cannot be reactivated (compensation only — new contract)

## Errors
| Error | Condition |
|---|---|
| `ContractAlreadyRegisteredError` | Contract for same (publisherRoute, schemaRef, schemaVersion) already active |
| `ContractSchemaRefNotPublishedError` | SchemaRef references a Draft or Deprecated schema |
| `ContractSubscriberConstraintInvalidError` | MinSchemaVersion < 1 or CompatibilityMode unknown |
| `ContractDeprecatedError` | Subscriber addition attempted on deprecated contract |

## Commands
| Command | Description |
|---|---|
| `RegisterContract` | Register a new schema contract |
| `AddContractSubscriber` | Add a subscriber constraint to an existing contract |
| `DeprecateContract` | Deprecate a contract |

## Queries
| Query | Returns |
|---|---|
| `GetContract(contractId)` | Single contract with all constraints |
| `GetContractByPublisher(publisherRoute)` | Active contract for a publisher |

## Projection Needs
`ContractView`: contractId, contractName, publisherRoute, schemaRef, schemaVersion, subscribers[], status

## Kafka Topic
`whyce.platform.schema.contract.events`

## Dependencies
- `core-system/identifier` — IIdGenerator for ContractId
- `schema/schema-definition` — SchemaRef target (cross-domain reference via ID only)
