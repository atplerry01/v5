# platform-system / schema / versioning

**Classification:** platform-system  
**Context:** schema  
**Domain:** versioning

## Purpose
Owns the version evolution model for schemas. Defines the rules governing how a schema
version can evolve from one version to the next — classifying changes as non-breaking,
breaking, or incompatible, and issuing a compatibility verdict that must align with the
schema's declared CompatibilityMode.

## Aggregate: VersioningRuleAggregate

**Identity:** `VersioningRuleId` — SHA256 of (schemaRef + fromVersion + toVersion)

**State:**
| Field | Type | Description |
|---|---|---|
| `VersioningRuleId` | value object (Guid) | Deterministic versioning rule ID |
| `SchemaRef` | value object (Guid) | References schema-definition ID |
| `FromVersion` | int | Source version |
| `ToVersion` | int | Target version (must be > FromVersion) |
| `EvolutionClass` | enum value object | NonBreaking \| Breaking \| Incompatible |
| `ChangeSummary` | collection (SchemaChange) | Individual field-level changes |
| `CompatibilityVerdict` | enum | Compatible \| ConditionallyCompatible \| Incompatible |

## Value Object: SchemaChange
| Field | Type | Values |
|---|---|---|
| `ChangeType` | enum | FieldAdded \| FieldRemoved \| FieldTypeChanged \| FieldRequiredChanged \| FieldRenamed |
| `FieldName` | string | Affected field name |
| `Impact` | enum | Safe \| RequiresConsumerUpdate \| Breaking |

## Breaking Change Classification Rules
| Change | Classification |
|---|---|
| Adding an optional field | NonBreaking (Safe) |
| Adding a required field without default | Breaking |
| Removing a required field | Breaking |
| Removing an optional field | NonBreaking (RequiresConsumerUpdate) |
| Changing field type | Breaking |
| Renaming a field | Breaking |

## Events
| Event | Trigger |
|---|---|
| `VersioningRuleRegisteredEvent` | Versioning rule registered for a schema version pair |
| `VersioningRuleVerdictIssuedEvent` | Compatibility verdict issued |

## Invariants
- VersioningRuleId deterministic from (schemaRef + fromVersion + toVersion)
- ToVersion must be strictly greater than FromVersion
- Only one versioning rule may exist per (schemaRef, fromVersion, toVersion)
- EvolutionClass must be consistent with the aggregate ChangeSummary impact
- If schemaRef declares CompatibilityMode = Backward, then EvolutionClass must be
  NonBreaking — a Breaking class in a Backward-compatible schema is rejected
- CompatibilityVerdict aligns with CompatibilityMode declared in schema-definition
- ChangeSummary must be non-empty (at least one change declared)

## Errors
| Error | Condition |
|---|---|
| `VersioningRuleConflictError` | Rule already exists for (schemaRef, fromVersion, toVersion) |
| `BreakingChangeInBackwardCompatibleSchemaError` | Breaking change registered for Backward-mode schema |
| `InvalidVersionOrderError` | ToVersion ≤ FromVersion |
| `EmptyChangeSummaryError` | No changes declared in ChangeSummary |

## Commands
| Command | Description |
|---|---|
| `RegisterVersioningRule` | Register version evolution rule for a schema version pair |
| `IssueVersioningVerdict` | Issue and record the final compatibility verdict |

## Queries
| Query | Returns |
|---|---|
| `GetVersioningRule(schemaRef, fromVersion, toVersion)` | Single versioning rule |
| `CheckVersionCompatibility(schemaRef, producerVersion, consumerVersion)` | Compatibility check |

## Value Object: VersionCompatibilitySpec
Encapsulates a compatibility check input:
- `SchemaRef` (Guid)
- `ProducerVersion` (int)
- `ConsumerVersion` (int)
- `IsCompatible` (bool — derived from versioning rule lookup)

## Projection Needs
`VersioningRuleView`: versioningRuleId, schemaRef, fromVersion, toVersion, evolutionClass, verdict, changeSummary[]

## Kafka Topic
`whyce.platform.schema.versioning.events`

## Dependencies
- `core-system/identifier` — IIdGenerator for VersioningRuleId
- `schema/schema-definition` — SchemaRef target (cross-domain reference via ID only)
