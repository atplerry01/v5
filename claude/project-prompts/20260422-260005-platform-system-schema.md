---
TITLE: platform-system / schema — E1→EX Canonical Batch
CLASSIFICATION: platform-system
CONTEXT: schema
DOMAIN GROUP: schema (3-level form — no physical domain-group folder segment)
DOMAINS:
  - schema-definition
  - contract
  - versioning
  - serialization
BATCH_DESCRIPTION: >
  Defines contract integrity and versioning for system-wide communication. The schema
  context owns the canonical structural model for schema definitions (schema-definition),
  the contract binding schemas to publishers and subscribers (contract), the version
  evolution rules (versioning), and the serialization format specifications (serialization).
  Schema must be explicit, versioned, and registry-controlled — never inferred at runtime.
SOURCE: claude/project-topics/v3/platform-system.md
TEMPLATE: claude/templates/pipelines/generic-prompt.md
---

# WBSM $3 MANDATORY SECTIONS

## TITLE
platform-system / schema — E1→EX Canonical Domain Batch

## CONTEXT
The `schema` context of platform-system owns the structural model for message contracts,
versioning, and serialization rules.

Current state:
- schema-definition: ✓ scaffolded
- contract: MISSING — must be added per platform-system topic
- versioning: MISSING — must be added per platform-system topic
- serialization: MISSING — must be added per platform-system topic

Schema invariants (per platform-system topic §7):
- Schemas MUST be versioned
- Schemas MUST be backward-compatible where required
- Schemas MUST be explicit
- Schemas MUST NOT be inferred at runtime
- Schemas MUST be contractually enforced

Physical path: `src/domain/platform-system/schema/{domain}/`
Form: 3-level (no domain-group segment)

## OBJECTIVE
1. Add 3 missing schema context domains: contract, versioning, serialization.
2. Update schema-definition to full platform-system topic specification.
3. Deliver full E1→EX for the schema context.

## CONSTRAINTS
- Schema context depends only on core-system (identifier)
- No runtime inference — schemas must be explicitly declared and registered
- Backward-compatibility rules must be declarative and explicit in versioning domain
- No business semantics in schema definitions
- No Guid.NewGuid(), no DateTime.UtcNow — IIdGenerator + IClock
- Domain layer: zero external dependencies

## EXECUTION STEPS

### STAGE E1 — DOMAIN MODEL

#### schema-definition
Purpose: Canonical schema structure — declares the fields, types, required set, version,
and compatibility mode for any message payload.

Aggregate: SchemaDefinitionAggregate (update existing scaffold)
- SchemaDefinitionId (SHA256 of name+version)
- SchemaName (non-empty string value object)
- Fields (collection of FieldDescriptor value objects)
  - FieldDescriptor: name (string), fieldType (enum: String|Int|Long|Bool|Float|Bytes|
      Nested|Array|Map), required (bool), description (string), defaultValue (optional)
- Version (int, monotonically increasing)
- CompatibilityMode (enum: Backward | Forward | Full | None)
- Status: Draft | Published | Deprecated
Events:
- SchemaDefinitionDraftedEvent
- SchemaDefinitionPublishedEvent
- SchemaDefinitionDeprecatedEvent
Invariants:
- SchemaDefinitionId deterministic from name+version
- At least one field declared
- Published schemas are IMMUTABLE — no field mutation after publication
- Deprecated schemas cannot be re-published (compensation as new version)
- CompatibilityMode explicit
- SchemaName stable across versions (same name, incremented version)
Errors:
- SchemaDefinitionAlreadyPublishedError
- SchemaDefinitionFieldRequiredError
- SchemaDefinitionImmutableError

#### contract
Purpose: A contract binds a schema-definition to a publisher and one or more subscriber
constraints. Contracts are the enforcement mechanism — they ensure publishers emit
schema-conformant messages and subscribers declare their schema compatibility expectations.

Aggregate: ContractAggregate
- ContractId (SHA256 of publisherRoute+schemaRef+version)
- ContractName (non-empty string value object)
- PublisherRoute (DomainRoute value object — the system that publishes this schema)
- SchemaRef (references SchemaDefinitionId)
- SchemaVersion (int — pinned schema version)
- SubscriberConstraints (collection of SubscriberConstraint value objects)
  - SubscriberConstraint: subscriberRoute (DomainRoute), minSchemaVersion (int),
      requiredCompatibilityMode (CompatibilityMode enum)
- Status: Active | Deprecated
Events:
- ContractRegisteredEvent
- ContractSubscriberAddedEvent
- ContractDeprecatedEvent
Invariants:
- ContractId deterministic
- PublisherRoute is valid DomainRoute
- SchemaRef references a Published schema-definition
- Every SubscriberConstraint declares minSchemaVersion >= 1
- Deprecated contracts cannot be re-activated
Errors:
- ContractAlreadyRegisteredError
- ContractSchemaRefNotPublishedError
- ContractSubscriberConstraintInvalidError

#### versioning
Purpose: Version evolution model — declares the rules governing how schema versions
can evolve. Encodes what changes are breaking vs non-breaking and validates a proposed
schema evolution against declared rules.

Aggregate: VersioningRuleAggregate
- VersioningRuleId (SHA256 of schemaRef+fromVersion+toVersion)
- SchemaRef (references SchemaDefinitionId)
- FromVersion (int)
- ToVersion (int — must be > FromVersion)
- EvolutionClass (enum: NonBreaking | Breaking | Incompatible)
- ChangeSummary (collection of SchemaChange value objects)
  - SchemaChange: changeType (FieldAdded|FieldRemoved|FieldTypeChanged|FieldRequiredChanged|
      FieldRenamed), fieldName (string), impact (enum: Safe|RequiresConsumerUpdate|Breaking)
- CompatibilityVerdict (enum: Compatible | ConditionallyCompatible | Incompatible)
Events:
- VersioningRuleRegisteredEvent
- VersioningRuleVerdictIssuedEvent
Invariants:
- VersioningRuleId deterministic
- ToVersion > FromVersion
- EvolutionClass consistent with ChangeSummary items
- Removing a required field = Breaking
- Adding a required field without default = Breaking
- Adding an optional field = NonBreaking
- CompatibilityVerdict must align with CompatibilityMode declared in schema-definition
Errors:
- VersioningRuleConflictError (already exists for same version pair)
- BreakingChangeInBackwardCompatibleSchemaError

Value Object: VersionCompatibilitySpec
- SchemaName (string)
- ProducerVersion (int)
- ConsumerVersion (int)
- IsCompatible (bool) — derived from VersioningRule lookup

#### serialization
Purpose: Serialization format specification — declares encoding, options, schema mapping,
and round-trip integrity rules for converting messages to/from bytes.

Aggregate: SerializationFormatAggregate
- SerializationFormatId (SHA256 of formatName+encoding+version)
- FormatName (non-empty string value object — e.g. "whyce.json.v1")
- Encoding (enum: Json | Avro | Protobuf | MsgPack | Binary)
- SchemaRef (references SchemaDefinitionId — optional; required for Avro/Protobuf)
- Options (collection of SerializationOption value objects)
  - SerializationOption: key (string), value (string) — e.g. dateFormat, nullHandling
- RoundTripGuarantee (enum: Lossless | LossyWithDocumentedFields)
- Version (int)
- Status: Active | Deprecated
Events:
- SerializationFormatRegisteredEvent
- SerializationFormatDeprecatedEvent
Invariants:
- SerializationFormatId deterministic
- FormatName non-empty
- Encoding explicit
- Avro + Protobuf formats MUST reference a SchemaRef
- RoundTripGuarantee explicit — lossless by default unless documented exception
- Options are structural only — no business logic encoded in options
Errors:
- SerializationFormatMissingSchemaRefError (Avro/Protobuf without SchemaRef)
- SerializationFormatAlreadyRegisteredError

### STAGE E2 — COMMAND LAYER
schema-definition: DraftSchemaDefinition, PublishSchemaDefinition, DeprecateSchemaDefinition
contract: RegisterContract, AddContractSubscriber, DeprecateContract
versioning: RegisterVersioningRule, IssueVersioningVerdict
serialization: RegisterSerializationFormat, DeprecateSerializationFormat

### STAGE E3 — QUERY LAYER
GetSchemaDefinition(name, version), ListSchemaDefinitions(status?)
GetContract(contractId), GetContractByPublisher(publisherRoute)
GetVersioningRule(schemaRef, fromVersion, toVersion), CheckVersionCompatibility(spec)
GetSerializationFormat(formatName, version), ListSerializationFormats(encoding?)

### STAGE E4 — T2E ENGINE HANDLERS
Standard T2E. VersioningRule handler validates evolution class against declared field changes.
Breaking-change detection must be deterministic given the two schema versions as input.

### STAGE E5 — POLICY INTEGRATION
No policy ownership.
Policy action names (informational): platform.schema.definition.publish,
platform.schema.contract.register, platform.schema.versioning.issue

### STAGE E6 — EVENT FABRIC
Topics:
- whyce.platform.schema.schema-definition.events
- whyce.platform.schema.contract.events
- whyce.platform.schema.versioning.events
- whyce.platform.schema.serialization.events

### STAGE E7 — PROJECTIONS
- SchemaDefinitionView (name, version, fields[], compatibilityMode, status)
- ContractView (contractId, publisherRoute, schemaRef, schemaVersion, subscribers[], status)
- VersioningRuleView (schemaRef, fromVersion, toVersion, evolutionClass, verdict)
- SerializationFormatView (formatName, encoding, schemaRef, roundTripGuarantee, status)
- SchemaRegistryView (aggregate: schema catalog for fast lookup by name+version)

### STAGE E8 — API LAYER
POST/GET /api/platform/schema/definitions
POST   /api/platform/schema/definitions/{name}/{version}/publish
DELETE /api/platform/schema/definitions/{name}/{version}         (deprecate)
POST/GET /api/platform/schema/contracts
POST/GET /api/platform/schema/versioning-rules
POST /api/platform/schema/versioning-rules/check-compatibility
POST/GET /api/platform/schema/serialization-formats
DELETE /api/platform/schema/serialization-formats/{name}/{version}

### STAGE E9 — WORKFLOW
No T1M workflow. All direct T2E.

### STAGE E10 — OBSERVABILITY
Metrics:
- whyce.platform.schema.definition.published.total
- whyce.platform.schema.contract.registered.total
- whyce.platform.schema.versioning.breaking-change.total
- whyce.platform.schema.serialization.format.registered.total

### STAGE E11 — SECURITY
Service identity required for schema publication and contract registration.
Schema deprecation is irreversible — elevated trust required.

### STAGE E12 — E2E VALIDATION
- Draft → Publish schema-definition → verify immutability (publish attempt fails)
- Register contract referencing published schema → add subscriber constraint
- Register versioning rule (adding optional field) → verify NonBreaking verdict
- Register versioning rule (removing required field) → verify Breaking verdict
- Register serialization format (JSON) → verify round-trip guarantee
- Kafka: events on correct topics, projection updated

## OUTPUT FORMAT
Per domain: aggregate, value objects, events, errors, specifications, README.md
Also: update src/domain/platform-system/schema/README.md to list all 4 domains

## VALIDATION CRITERIA
- [ ] contract, versioning, serialization domains created
- [ ] schema-definition updated to include Draft status and field evolution
- [ ] schema/README.md lists all 4 domains
- [ ] Published schemas are immutable (enforced by invariant)
- [ ] BreakingChangeInBackwardCompatibleSchemaError defined
- [ ] Avro/Protobuf formats require SchemaRef (enforced)
- [ ] No Guid.NewGuid(), no DateTime.UtcNow
- [ ] Topics follow whyce.platform.schema.{domain}.events pattern
- [ ] SchemaRegistryView projection enables fast name+version lookup
