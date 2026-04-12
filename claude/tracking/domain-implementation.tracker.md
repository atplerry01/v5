# Domain Implementation Tracker — Phase 1.6

## Status Legend

| Code | Meaning |
|------|---------|
| S1 | Scaffold Only |
| S2 | Value Objects + Events |
| S3 | Aggregate with Apply methods |
| S4 | Invariants + Specifications Complete |
| S5 | Full Domain (services, cross-aggregate coordination) |

## business-system / agreement

| Domain | Status | Date | Notes |
|--------|--------|------|-------|
| acceptance | **S4** | 2026-04-11 | Gold standard reference implementation |
| amendment | **S4** | 2026-04-11 | Change management: Draft/Applied/Reverted, references target via AmendmentTargetId |
| approval | **S4** | 2026-04-11 | Decision domain: Pending/Approved/Rejected, authorization gate for agreements |
| clause | **S4** | 2026-04-11 | Structural domain with ClauseType classification |
| contract | **S4** | 2026-04-11 | High complexity: entity usage (ContractParty), 4-state lifecycle |
| counterparty | **S4** | 2026-04-11 | Relationship domain: Active/Suspended/Terminated, entity (CounterpartyProfile), DI-8 compliant |
| obligation | **S4** | 2026-04-11 | Medium complexity: Pending/Fulfilled/Breached lifecycle |
| renewal | **S4** | 2026-04-11 | Lifecycle extension: Pending/Renewed/Expired, references source via RenewalSourceId |
| signature | **S4** | 2026-04-11 | Authentication domain: Pending/Signed/Revoked, revocation only after signing |
| term | **S4** | 2026-04-11 | Time-bound: Draft/Active/Expired, TermDuration value object |
| validity | **S4** | 2026-04-11 | Constraint domain: Valid/Invalid/Expired, transitions only from Valid state |

## business-system / document

| Domain | Status | Date | Notes |
|--------|--------|------|-------|
| contract-document | **S4** | 2026-04-11 | Bound document: Draft/Finalized/Archived, entity (DocumentSection), immutable after finalization, linked to contract via ContractReferenceId |
| evidence | **S4** | 2026-04-11 | Immutable proof: Captured/Verified/Archived, entity (EvidenceArtifact), WhyceChain-aligned, immutable after capture |
| record | **S4** | 2026-04-11 | System record: Active/Locked/Archived, immutable after locking, historical integrity preserved |
| retention | **S4** | 2026-04-11 | Data lifecycle: Active/Retained/Expired, retention condition enforced before expiry |
| signature-record | **S4** | 2026-04-11 | Audit signature: Captured/Verified/Archived, entity (SignatureEntry), immutable after verification |
| template | **S4** | 2026-04-11 | Structured definition: Draft/Published/Deprecated, entity (TemplateStructure), immutable after publish |
| version | **S4** | 2026-04-11 | Version control: Draft/Released/Superseded, entity (VersionMetadata), deterministic VersionNumber, immutable after superseded |

## business-system / billing

| Domain | Status | Date | Notes |
|--------|--------|------|-------|
| invoice | **S4** | 2026-04-11 | Financial document: Draft/Issued/Paid/Cancelled, entity (InvoiceLineItem), DI-8 compliant |
| bill-run | **S4** | 2026-04-11 | Batch processing: Created/Running/Completed/Failed, entity (BillRunItem) |
| receivable | **S4** | 2026-04-11 | Financial position: Outstanding/Settled/WrittenOff, write-off irreversible |
| payment-application | **S4** | 2026-04-11 | Settlement domain: Pending/Applied/Reversed, entity (PaymentAllocation), invoice+payment source references, allocation integrity |
| adjustment | **S4** | 2026-04-11 | Financial correction: Draft/Applied/Voided, mandatory reason, no double-apply |
| statement | **S4** | 2026-04-11 | Aggregated view: Draft/Issued/Closed, entity (StatementLine), non-empty guard, close locks |

## business-system / entitlement

| Domain | Status | Date | Notes |
|--------|--------|------|-------|
| allocation | **S4** | 2026-04-11 | Resource assignment: Pending/Allocated/Released, resource reference enforced |
| eligibility | **S4** | 2026-04-11 | Rule evaluation: Pending/Eligible/Ineligible, deterministic evaluation only |
| entitlement-grant | **S4** | 2026-04-11 | Access grant: Pending/Granted/Revoked, entity (EntitlementAssignment), DI-8 compliant |
| limit | **S4** | 2026-04-11 | Boundary control: Defined/Enforced/Breached, threshold enforcement, breach requires exceeding threshold |
| quota | **S4** | 2026-04-11 | Usage cap: Available/Consumed/Exhausted, entity (QuotaUsage), capacity tracking, consumption guard |
| restriction | **S4** | 2026-04-11 | Access control: Active/Violated/Lifted, condition-based, deterministic violation, lift after violation only |
| revocation | **S4** | 2026-04-11 | Access termination: Active/Revoked/Finalized, typed RevocationTargetId, finalization locks permanently |
| right | **S4** | 2026-04-11 | Entitlement definition: Defined/Active/Deprecated, entity (RightDefinition), typed RightScopeId, DI-8 compliant |
| usage-right | **S4** | 2026-04-11 | Usage control: Available/InUse/Consumed, entity (UsageRecord), typed UsageRightSubjectId+ReferenceId, usage tracking |

## business-system / execution

| Domain | Status | Date | Notes |
|--------|--------|------|-------|
| activation | **S4** | 2026-04-11 | Start/enable: Pending/Active/Deactivated, typed ActivationTargetId, deactivation only after activation |
| allocation | **S4** | 2026-04-11 | Execution resource: Pending/Allocated/Released, typed ExecutionResourceId, RequestedCapacity enforced |
| charge | **S4** | 2026-04-11 | Execution cost: Pending/Charged/Reversed, entity (ChargeItem), typed CostSourceId, item-before-charge guard |
| completion | **S4** | 2026-04-11 | Execution outcome: Pending/Completed/Failed, typed CompletionTargetId, failure reason required |
| cost | **S4** | 2026-04-11 | Cost aggregation: Pending/Calculated/Finalized, entity (CostComponent), typed CostContextId, component-before-calculate guard |
| lifecycle | **S4** | 2026-04-11 | Lifecycle orchestration: Initialized/Running/Completed/Terminated, typed LifecycleSubjectId, no state skipping |
| milestone | **S4** | 2026-04-11 | Progression checkpoint: Defined/Reached/Missed, typed MilestoneTargetId, mutually exclusive terminals |
| setup | **S4** | 2026-04-11 | Initialization: Pending/Configured/Ready, typed SetupTargetId, sequential progression enforced |
| sourcing | **S4** | 2026-04-11 | Resource acquisition: Requested/Sourced/Failed, typed SourcingRequestId, failure reason required |
| stage | **S4** | 2026-04-11 | Workflow stage: Initialized/InProgress/Completed, typed StageContextId, strict progression enforced |

## business-system / integration

| Domain | Status | Date | Notes |
|--------|--------|------|-------|
| adapter | **S4** | 2026-04-11 | Integration adapter: Defined/Active/Disabled, typed AdapterTypeId, no infrastructure logic |
| client | **S4** | 2026-04-11 | External client: Registered/Active/Suspended, entity (ClientCredential), typed ExternalClientId, credential-before-activate guard |
| endpoint | **S4** | 2026-04-11 | Integration endpoint: Defined/Available/Unavailable, entity (EndpointDefinition), EndpointUri value object, bidirectional availability toggle |
| gateway | **S4** | 2026-04-11 | Integration entry: Defined/Active/Disabled, typed GatewayRouteId, route reference enforced |
| connector | **S4** | 2026-04-11 | System connection: Defined/Connected/Disconnected, typed ConnectorTargetId, disconnect follows connect |
| contract | **S4** | 2026-04-11 | Integration contract: Draft/Active/Terminated, entity (ContractSchema), typed ContractSchemaId, DI-8 compliant |
| provider | **S4** | 2026-04-11 | External provider: Registered/Active/Suspended, entity (ProviderProfile), typed ProviderConfigId, DI-8 compliant |
| event-bridge | **S4** | 2026-04-11 | Event mapping: Defined/Active/Disabled, REVERSIBLE lifecycle, typed EventMappingId, contract-only (no transport) |
| command-bridge | **S4** | 2026-04-11 | Command mapping: Defined/Active/Disabled, REVERSIBLE lifecycle, typed CommandMappingId, contract-only (no transport) |
| callback | **S4** | 2026-04-11 | Callback contract: Registered/Pending/Completed, SEQUENTIAL lifecycle, entity (CallbackDefinition), DI-8, contract-only |
| webhook | **S4** | 2026-04-11 | Webhook definition: Defined/Active/Disabled, REVERSIBLE lifecycle, entity (WebhookDefinition), DI-8, contract-only (no HTTP) |
| mapping | **S4** | 2026-04-11 | Transformation rules: Defined/Active/Disabled, REVERSIBLE lifecycle, typed MappingDefinitionId, contract-only (no execution) |
| retry | **S4** | 2026-04-11 | Retry policy: Defined/Active/Disabled, REVERSIBLE lifecycle, typed RetryPolicyId, contract-only (no execution/timers) |
| failure | **S4** | 2026-04-11 | Failure classification: Detected/Classified/Resolved, TERMINAL lifecycle, typed FailureTypeId, contract-only |
| replay | **S4** | 2026-04-11 | Replay eligibility: Defined/Active/Disabled, REVERSIBLE lifecycle, typed ReplayPolicyId, contract-only (no execution) |
| schema | **S4** | 2026-04-11 | Structure contract: Draft/Published/Finalized, TERMINAL lifecycle, typed SchemaDefinitionId, immutable once finalized |
| subscription | **S4** | 2026-04-11 | Participation/interest: Defined/Active/Deactivated, REVERSIBLE lifecycle, typed SubscriptionTargetId, contract-only |
| registry | **S4** | 2026-04-11 | Lookup/discovery: Defined/Active/Deactivated, REVERSIBLE lifecycle, typed RegistryEntryId, contract-only (no lookup execution) |
| synchronization | **S4** | 2026-04-11 | Consistency contract: Defined/Pending/Confirmed, SEQUENTIAL lifecycle, typed SyncPolicyId, contract-only (no sync jobs) |

## business-system / inventory

| Domain | Status | Date | Notes |
|--------|--------|------|-------|
| item | **S4** | 2026-04-11 | Identity definition: Active/Discontinued, TERMINAL lifecycle, typed ItemTypeId, immutable after discontinue |
| stock | **S4** | 2026-04-11 | Quantity state: Initialized/Tracked/Depleted, SEQUENTIAL lifecycle, Quantity VO (non-negative), typed StockItemId |
| movement | **S4** | 2026-04-11 | Stock changes: Pending/Confirmed/Cancelled, TERMINAL lifecycle, MovementQuantity VO (>0), typed Source+TargetId |
| reservation | **S4** | 2026-04-11 | Temporary allocation: Reserved/Confirmed/Released, REVERSIBLE lifecycle, ReservedQuantity VO (>0), typed ReservationItemId |

## structural-system / cluster

| Domain | Status | Date | Notes |
|--------|--------|------|-------|
| cluster | **S4** | 2026-04-12 | ROOT aggregate: Defined/Active/Archived, TERMINAL lifecycle, canonical pattern reference |
| authority | **S4** | 2026-04-12 | Governance: Established/Active/Revoked, TERMINAL lifecycle, ClusterReference |
| provider | **S4** | 2026-04-12 | Service provider: Registered/Active/Suspended, REVERSIBLE lifecycle, ClusterReference |
| subcluster | **S4** | 2026-04-12 | Nested hierarchy: Defined/Active/Archived, TERMINAL lifecycle, ParentClusterReference |
| lifecycle | **S4** | 2026-04-12 | State definition: Defined/Transitioned/Completed, TERMINAL lifecycle, ClusterReference |
| spv | **S4** | 2026-04-12 | Special Purpose Vehicle: Created/Active/Suspended/Closed, TERMINAL lifecycle, SpvType (Operating/Brand/Hybrid), ClusterReference |
| topology | **S4** | 2026-04-12 | Relationship mapping: Defined/Validated/Locked, TERMINAL lifecycle, ClusterReference, defines relationships (NOT execution) |

## structural-system / structure

| Domain | Status | Date | Notes |
|--------|--------|------|-------|
| classification | **S4** | 2026-04-12 | Category definition: Defined/Active/Deprecated, TERMINAL lifecycle, ClassificationDescriptor (name+category) |
| type-definition | **S4** | 2026-04-12 | Type schema: Defined/Active/Retired, TERMINAL lifecycle, TypeDefinitionDescriptor (name+category) |
| hierarchy-definition | **S4** | 2026-04-12 | Parent-child structure: Defined/Validated/Locked, TERMINAL lifecycle, self-referencing guard, ParentReference |
