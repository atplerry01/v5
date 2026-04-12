# economic.guard.md

## NEW RULES INTEGRATED — 2026-04-07 (NORMALIZATION)

### RULE: ECON-ES-01 — EVENT SOURCING IS SOURCE OF TRUTH
All economic state changes MUST occur via domain events.
No direct state mutation is allowed outside aggregate methods.

ENFORCEMENT:
- Aggregates must raise events before state change
- No repository-level mutation logic allowed

---

### RULE: ECON-CQRS-01 — READ/WRITE SEPARATION
Write model (aggregates) and read model (projections) MUST be separated.

ENFORCEMENT:
- src/domain MUST NOT depend on projections
- src/projections MUST NOT mutate domain state

---

### RULE: ECON-LEDGER-01 — INVARIANT ENFORCEMENT
Economic aggregates MUST enforce invariant checks BEFORE emitting events.

ENFORCEMENT:
- Ledger must balance
- Allocation/reserve must validate constraints before event emission

---

## TRANSACTION CONTEXT RULES — 2026-04-10

### T-RULES (Transaction Flow)

#### T1 — TRANSACTION OUTPUT RULE
Every transaction MUST produce exactly one journal. A completed `TransactionAggregate` with an empty `JournalId` is an invariant violation. Enforced in `TransactionAggregate.EnsureInvariants()`.

#### T2 — NO DIRECT ENTRY CREATION
Transaction cannot create entries directly. The flow is: Transaction → Journal → Entries. Any code path in the transaction context that creates `LedgerEntryAggregate` instances or raises `LedgerEntryRecordedEvent` is a **CRITICAL violation**.

#### T3 — LIMIT VALIDATION FIRST
Transaction must validate limits before execution. `LimitAggregate.Check()` must be called BEFORE `TransactionAggregate.Complete()`. Calling `Complete()` without prior limit validation is a flow violation.

#### T4 — CHARGE BEFORE EXECUTION
Charges must be calculated and applied before journal creation. `ChargeAggregate.Calculate()` and `ChargeAggregate.ApplyCharge()` must complete BEFORE `TransactionAggregate.Complete()`. Charges feed into the journal entries.

#### T5 — INSTRUCTION REQUIRED
Transaction must originate from an instruction. `TransactionAggregate.Initiate()` requires a non-empty `InstructionId`. A transaction without an instruction reference is a **CRITICAL violation**.

### Transaction D-RULES (Domain Constraints)

#### D41 — TRANSACTION REQUIRES JOURNAL
Every transaction must produce a journal. Validated by `TransactionJournalLinkService.ValidateJournalProduced()` and enforced as an aggregate invariant.

#### D42 — NO ENTRY CREATION IN TRANSACTION
Entries must only be created via journal. The transaction context MUST NOT contain any reference to `LedgerEntryAggregate`, `LedgerEntryRecordedEvent`, or entry-level domain types.

#### D43 — INSTRUCTION-FIRST FLOW
Transactions must originate from instruction. `TransactionAggregate.Initiate()` enforces non-empty `InstructionId`.

#### D44 — LIMIT ENFORCEMENT
Transactions must validate limits before execution. `LimitCheckedEvent` → `TransactionCompletedEvent`. If `LimitExceededEvent`, the transaction MUST NOT complete.

#### D45 — CHARGE APPLICATION
Charges must be applied before execution. `ChargeCalculatedEvent` → `ChargeAppliedEvent` → `TransactionCompletedEvent`.

### Transaction C-CONSTRAINTS (Violation Codes)

| Code | Violation | Severity |
|---|---|---|
| **C26** | Transaction completed without journal | S0 — CRITICAL |
| **C27** | Entry created outside journal flow | S0 — CRITICAL |
| **C28** | Transaction created without instruction | S0 — CRITICAL |
| **C29** | Transaction completed without limit validation | S1 — HIGH |
| **C30** | Transaction completed without charge application | S1 — HIGH |

### CANONICAL TRANSACTION EXECUTION ORDER

```
1. Instruction created          (TransactionInstructionCreatedEvent)
2. Limits checked               (LimitCheckedEvent)
3. Charge calculated            (ChargeCalculatedEvent)
4. Charge applied               (ChargeAppliedEvent)
5. Transaction initiated        (TransactionInitiatedEvent)
6. Transaction completed        (TransactionCompletedEvent — includes JournalId)
7. Instruction marked executed  (TransactionInstructionExecutedEvent)
```

Steps 2-4 MUST complete before step 6. Step 1 MUST precede step 5. Step 6 MUST include a journal reference.

### Transaction Check Procedure

1. Scan `TransactionAggregate.Complete()` — verify it requires non-empty `JournalId` parameter.
2. Scan `TransactionAggregate.Initiate()` — verify it requires non-empty `InstructionId`.
3. Scan `TransactionAggregate.EnsureInvariants()` — verify Completed status requires JournalId.
4. Grep transaction context for `LedgerEntryAggregate` or `LedgerEntryRecordedEvent` — must find zero results.
5. Verify `LimitAggregate.Check()` raises `LimitExceededEvent` and throws on breach.
6. Verify `ChargeAggregate.ApplyCharge()` transitions from Calculated to Applied.

### Transaction Fail Criteria

- `TransactionAggregate` allows completion without `JournalId` → **C26**
- Any file in `src/domain/economic-system/transaction/` references entry-level types → **C27**
- `TransactionAggregate` allows initiation without `InstructionId` → **C28**
- No limit check mechanism exists or is bypassable → **C29**
- Charge can be skipped in execution flow → **C30**

---

## REVENUE CONTEXT RULES — 2026-04-10

### R-RULES (Revenue Flow)

#### R1 — CONTRACT REQUIRED
Revenue cannot exist without a contract. `RevenueAggregate.Recognize()` requires a non-empty `ContractId`. Revenue without a contract origin is a **CRITICAL violation**.

#### R2 — SHARE CONSISTENCY
Distribution must equal total revenue. `DistributionAggregate.EnsureInvariants()` enforces that the sum of all allocation amounts cannot exceed `TotalAmount`. `DistributionSplitService.ValidateAllocationsSum()` validates exact equality.

#### R3 — PAYOUT CONSISTENCY
Payout must match distribution exactly. `PayoutAggregate.Initiate()` requires a non-empty `DistributionId`. `PayoutMatchingService.ValidatePayoutMatchesDistribution()` validates the link.

#### R4 — NO DIRECT PAYMENT
Revenue cannot directly trigger ledger entries. The flow is: Revenue → Distribution → Payout → Transaction → Journal → Entries. Any code path in the revenue context that creates journal entries or references ledger-level types is a **CRITICAL violation**.

#### R5 — TRANSACTION REQUIRED
Payout must go through the transaction context. Revenue payouts produce transactions, which produce journals, which produce entries. Payout does not write to ledger directly.

### Revenue D-RULES (Domain Constraints)

#### D46 — CONTRACT FOUNDATION
All revenue must originate from a contract. `RevenueAggregate.Recognize()` enforces non-empty `ContractId`. `RevenueTraceService.ValidateOrigin()` validates the link. A revenue record without a contract reference is structurally invalid.

#### D47 — DISTRIBUTION SUM RULE
Allocations must equal total revenue. `DistributionAggregate.EnsureInvariants()` enforces sum of allocations <= total. `IsFullyAllocatedSpecification` validates exact equality. Over-allocation is an invariant violation.

#### D48 — PAYOUT MATCH RULE
Payout must match distribution. `PayoutAggregate` requires non-empty `DistributionId`. Payouts without a distribution reference are structurally invalid.

#### D49 — NO DIRECT LEDGER WRITE
Revenue cannot write to ledger directly. The revenue context (`src/domain/economic-system/revenue/`) MUST NOT contain any reference to `LedgerEntryAggregate`, `LedgerEntryRecordedEvent`, `JournalAggregate`, or ledger-level domain types. Revenue flows through transaction, not ledger.

#### D50 — TRANSACTION FLOW REQUIRED
Payout must go through transaction. The canonical flow is: `PayoutInitiatedEvent` → (transaction context handles execution) → `PayoutCompletedEvent`. Revenue context never directly interacts with the ledger context.

### Revenue C-CONSTRAINTS (Violation Codes)

| Code | Violation | Severity |
|---|---|---|
| **C31** | Revenue without contract reference | S0 — CRITICAL |
| **C32** | Distribution allocations do not match total revenue | S0 — CRITICAL |
| **C33** | Payout without distribution reference | S0 — CRITICAL |
| **C34** | Direct ledger write from revenue context | S0 — CRITICAL |
| **C35** | Payout without transaction linkage | S1 — HIGH |

### CANONICAL REVENUE EXECUTION ORDER

```
1. RevenueContractCreatedEvent       → Agreement drafted
2. RevenueContractActivatedEvent     → Contract active
3. PriceDefinedEvent                 → Value determined
4. RevenueRecognizedEvent            → Earned value recognized (R1 — requires ContractId)
5. DistributionCreatedEvent          → Revenue split initiated
6. AllocationAssignedEvent (x N)     → Per-party allocations assigned (R2 — sum must match)
7. RevenueDistributedEvent           → Revenue marked distributed
8. PayoutInitiatedEvent (x N)        → Payouts created (R3 — must reference distribution)
9. [Transaction context]             → Payout triggers transaction (R5)
10. PayoutCompletedEvent             → Payment finalized
```

Steps 1-3 MUST precede step 4. Step 4 MUST precede step 5. Step 6 sum MUST equal step 5 total. Step 8 MUST reference step 5.

### Revenue Check Procedure

1. Scan `RevenueAggregate.Recognize()` — verify it requires non-empty `ContractId`.
2. Scan `DistributionAggregate.EnsureInvariants()` — verify allocation sum check.
3. Scan `PayoutAggregate.Initiate()` — verify it requires non-empty `DistributionId`.
4. Grep revenue context for `LedgerEntryAggregate`, `LedgerEntryRecordedEvent`, `JournalAggregate` — must find zero results.
5. Verify `PayoutAggregate` does not reference any ledger or journal types.
6. Verify `RevenueTraceService.ValidateOrigin()` checks contract reference.

### Revenue Fail Criteria

- `RevenueAggregate` allows recognition without `ContractId` → **C31**
- `DistributionAggregate` allows finalization with mismatched allocations → **C32**
- `PayoutAggregate` allows initiation without `DistributionId` → **C33**
- Any file in `src/domain/economic-system/revenue/` references ledger types → **C34**
- Payout can complete without going through transaction context → **C35**

---

## EXPOSURE CONTEXT RULES — 2026-04-10

### X-RULES (Cross-Domain Exposure)

#### X1 — EXPOSURE SOURCE RULE
All exposure must originate from one of: allocation, obligation, or transaction. `ExposureAggregate.Create()` requires a non-empty `SourceId` and a valid `ExposureType` (Allocation, Obligation, Transaction). Exposure without a source origin is a **CRITICAL violation**.

#### X2 — NO ORPHAN EXPOSURE
Exposure must always reference a valid economic source. An `ExposureAggregate` with an empty `SourceId` is an invariant violation. No exposure record may exist without a traceable link to a real economic action.

#### X3 — EXPOSURE THRESHOLD RULE
Exposure must not exceed defined limits. `ExposureThresholdSpecification.IsSatisfiedBy()` must be evaluated before any exposure increase is accepted. Exceeding the threshold without validation is a flow violation.

#### X4 — EXPOSURE TRACEABILITY
Exposure must be reconstructable from source events. The aggregate state is derived by replaying `ExposureCreatedEvent`, `ExposureIncreasedEvent`, `ExposureReducedEvent`, and `ExposureClosedEvent`. Any state not derivable from events is a violation.

#### X5 — NON-MUTATIVE RULE
Exposure does not change capital directly. The exposure context (`src/domain/economic-system/risk/exposure/`) MUST NOT contain any reference to `VaultAggregate`, `LedgerEntryAggregate`, `JournalAggregate`, `CapitalPoolAggregate`, or any capital/ledger domain types. Exposure reflects risk — it does not move funds.

### Exposure D-RULES (Domain Constraints)

#### D51 — EXPOSURE DOMAIN REQUIRED
The `risk/exposure` domain must exist under `economic-system` with a complete DDD structure: aggregate/, entity/, error/, event/, service/, specification/, value-object/. Missing domain is a structural violation.

#### D52 — SOURCE LINKAGE
Exposure must reference allocation, obligation, or transaction. `ExposureAggregate.Create()` enforces non-empty `SourceId` and valid `ExposureType`. An exposure without a source reference is structurally invalid.

#### D53 — NO ORPHAN EXPOSURE
Exposure without a source is forbidden. `SourceId` is validated at creation via `Guard.Against(value == Guid.Empty)`. Any code path that creates an `ExposureAggregate` without a valid `SourceId` is a **CRITICAL violation**.

#### D54 — THRESHOLD ENFORCEMENT
Exposure must enforce limits. `ExposureThresholdSpecification.IsSatisfiedBy(currentExposure, threshold)` must be available and callable before exposure increases. Bypassing threshold validation is a flow violation.

#### D55 — NON-MUTATIVE RULE
Exposure cannot modify capital or ledger. The exposure context MUST NOT contain any reference to capital aggregates (`VaultAggregate`, `CapitalPoolAggregate`), ledger types (`LedgerEntryAggregate`, `JournalAggregate`), or transaction execution types. Exposure is read-only risk visibility.

### Exposure C-CONSTRAINTS (Violation Codes)

| Code | Violation | Severity |
|---|---|---|
| **C36** | Missing exposure domain (`risk/exposure` not found) | S0 — CRITICAL |
| **C37** | Exposure without source reference (`SourceId` empty) | S0 — CRITICAL |
| **C38** | Exposure exceeding threshold without validation | S1 — HIGH |
| **C39** | Orphan exposure detected (no traceable source) | S0 — CRITICAL |
| **C40** | Exposure modifying capital/ledger directly | S0 — CRITICAL |

### CANONICAL EXPOSURE EVENT FLOW

```
1. Source event occurs              (AllocationAssignedEvent / ObligationCreatedEvent / TransactionInitiatedEvent)
2. Exposure created                 (ExposureCreatedEvent — X1, requires SourceId + ExposureType)
3. Exposure increased (optional)    (ExposureIncreasedEvent — X3, threshold must be checked)
4. Exposure reduced (optional)      (ExposureReducedEvent — amount <= current exposure)
5. Exposure closed                  (ExposureClosedEvent — zeroes exposure, sets Closed status)
```

Step 2 MUST include a valid SourceId and ExposureType (X1, D52). Step 3 MUST validate against threshold (X3, D54). Step 5 is terminal — no further mutations after Closed status.

### Exposure Check Procedure

1. Verify `src/domain/economic-system/risk/exposure/` exists with all 7 mandatory subfolders — D51.
2. Scan `ExposureAggregate.Create()` — verify it requires non-empty `SourceId` and valid `ExposureType` — D52, D53.
3. Verify `ExposureThresholdSpecification.IsSatisfiedBy()` exists and accepts exposure + threshold — D54.
4. Grep exposure context for `VaultAggregate`, `CapitalPoolAggregate`, `LedgerEntryAggregate`, `JournalAggregate` — must find zero results — D55.
5. Verify `SourceId` constructor rejects `Guid.Empty` — D53.
6. Verify all aggregate state changes go through `RaiseDomainEvent` — X4.

### Exposure Fail Criteria

- `risk/exposure` domain missing or incomplete → **C36**
- `ExposureAggregate` allows creation without `SourceId` → **C37**
- Exposure can increase without threshold validation path → **C38**
- `SourceId` accepts `Guid.Empty` → **C39**
- Any file in `src/domain/economic-system/risk/exposure/` references capital/ledger types → **C40**

---

## RECONCILIATION CONTEXT RULES — 2026-04-10

### RC-RULES (Reconciliation Flow)

#### RC1 — LEDGER AUTHORITATIVE
Reconciliation validates alignment between the ledger (source of truth) and observed/derived states. Reconciliation MUST NOT modify ledger state. The reconciliation context (`src/domain/economic-system/reconciliation/`) MUST NOT contain any reference to `LedgerEntryAggregate`, `JournalAggregate`, `VaultAggregate`, or any write-side ledger types.

#### RC2 — RESULT REQUIRED
Every reconciliation process must produce a result. `ProcessAggregate` must transition to either `Matched` or `Mismatched` before it can be resolved. Calling `Resolve()` on a process in `Pending` or `InProgress` status is a flow violation.

#### RC3 — DISCREPANCY TRACEABILITY
Every discrepancy must reference a reconciliation process. `DiscrepancyAggregate.Detect()` requires a non-empty `ProcessReference`. A discrepancy without a process origin is a **CRITICAL violation**.

#### RC4 — NO IGNORED DISCREPANCY
Discrepancies must not be silently dropped. Every detected discrepancy must be either acknowledged or resolved. The system must not permit deletion or silent removal of discrepancy records.

#### RC5 — NON-MUTATIVE
Reconciliation does not create or modify financial truth. The reconciliation context reads and compares — it never writes to capital, ledger, or transaction contexts.

### Reconciliation D-RULES (Domain Constraints)

#### D56 — RECONCILIATION DOMAINS REQUIRED
The `reconciliation` context must contain two domains: `process` and `discrepancy`. Each domain must have all 7 mandatory DDD subfolders. Missing domain is a structural violation.

#### D57 — PROCESS MUST PRODUCE RESULT
`ProcessAggregate.Resolve()` enforces that status is `Matched` or `Mismatched` before resolution. Resolving a `Pending` or `InProgress` process is forbidden.

#### D58 — DISCREPANCY REQUIRES PROCESS
`DiscrepancyAggregate.Detect()` enforces non-empty `ProcessReference`. A discrepancy without a process reference is structurally invalid.

#### D59 — NO LEDGER MUTATION
The reconciliation context MUST NOT reference write-side ledger types (`LedgerEntryAggregate`, `JournalAggregate`, `VaultAggregate`). Reconciliation is read-only comparison.

#### D60 — DISCREPANCY LIFECYCLE
Discrepancies follow the lifecycle: Open → Investigating → Resolved. `Investigate()` requires `Open` status. `Resolve()` requires `Open` or `Investigating` status. Resolved discrepancies are terminal.

#### D61 — COMPARISON DATA REQUIRED
`DiscrepancyAggregate.Detect()` requires `ExpectedValue` (from ledger) and `ActualValue` (observed). The `Difference` is computed as `ExpectedValue - ActualValue`. A discrepancy without comparison data is a **CRITICAL violation**.

#### D62 — DISCREPANCY SOURCE REQUIRED
Every discrepancy must declare its source: `Projection` or `ExternalSystem`. The `DiscrepancySource` enum is mandatory at detection time.

### Reconciliation C-CONSTRAINTS (Violation Codes)

| Code | Violation | Severity |
|---|---|---|
| **C41** | Missing reconciliation domain (`process` or `discrepancy` not found) | S0 — CRITICAL |
| **C42** | Reconciliation resolved without result (Pending/InProgress → Resolved) | S0 — CRITICAL |
| **C43** | Discrepancy without process reference | S0 — CRITICAL |
| **C44** | Reconciliation context modifying ledger state | S0 — CRITICAL |
| **C45** | Discrepancy silently dropped or deleted | S1 — HIGH |
| **C46** | Discrepancy without comparison data (ExpectedValue/ActualValue) | S0 — CRITICAL |
| **C47** | Discrepancy without source declaration | S0 — CRITICAL |

### CANONICAL RECONCILIATION EVENT FLOW

```
1. ReconciliationTriggeredEvent      → Process initiated with ledger + observed references
2. [Comparison logic]                → Engine/runtime performs comparison
3a. ReconciliationMatchedEvent       → States align (RC2 — result produced)
3b. ReconciliationMismatchedEvent    → States diverge (RC2 — result produced)
4. DiscrepancyDetectedEvent (if 3b)  → Mismatch tracked (RC3 — requires ProcessReference, ExpectedValue, ActualValue)
5. DiscrepancyInvestigatedEvent      → Mismatch under investigation (Open → Investigating)
6. DiscrepancyResolvedEvent          → Mismatch resolved with resolution description
7. ReconciliationResolvedEvent       → Process finalized (RC2 — requires result first)
```

Step 3 MUST precede step 7. Step 4 MUST reference step 1. Step 6 MUST include a resolution description.

### Reconciliation Check Procedure

1. Verify `src/domain/economic-system/reconciliation/process/` and `src/domain/economic-system/reconciliation/discrepancy/` exist with all 7 mandatory subfolders — D56.
2. Scan `ProcessAggregate.Resolve()` — verify it rejects `Pending` and `InProgress` status — D57.
3. Scan `DiscrepancyAggregate.Detect()` — verify it requires non-empty `ProcessReference` — D58.
4. Grep reconciliation context for `LedgerEntryAggregate`, `JournalAggregate`, `VaultAggregate` — must find zero results — D59.
5. Verify `DiscrepancyAggregate` state transitions follow Open → Investigating → Resolved — D60.
6. Verify `ProcessReference` constructor rejects `Guid.Empty` — D58.
7. Verify `DiscrepancyAggregate.Detect()` requires `ExpectedValue` and `ActualValue` parameters and computes `Difference` — D61.
8. Verify `DiscrepancyAggregate.Detect()` requires `DiscrepancySource` parameter — D62.

### Reconciliation Fail Criteria

- `reconciliation/process` or `reconciliation/discrepancy` domain missing → **C41**
- `ProcessAggregate` allows resolution without prior result → **C42**
- `DiscrepancyAggregate` allows detection without `ProcessReference` → **C43**
- Any file in `src/domain/economic-system/reconciliation/` references write-side ledger types → **C44**
- Discrepancy records can be deleted or dropped without resolution → **C45**
- `DiscrepancyAggregate` allows detection without `ExpectedValue`/`ActualValue` → **C46**
- `DiscrepancyAggregate` allows detection without `DiscrepancySource` → **C47**

---

## CROSS-DOMAIN RECONCILIATION RULES — 2026-04-10

These rules apply across all domain boundaries that interact with reconciliation. They reinforce and extend the context-specific RC-RULES above.

### CR-RULES (Cross-Domain)

#### CR1 — LEDGER AUTHORITY
Ledger is the single source of truth. Reconciliation always validates observed state against ledger state — never the reverse. Reinforces RC1.

#### CR2 — NO MUTATION
Reconciliation cannot modify ledger, capital, or transaction state. This extends RC5 to explicitly cover capital and transaction contexts in addition to ledger.

#### CR3 — DISCREPANCY REQUIRED
All mismatches between ledger and observed state must produce a discrepancy record. Silent discarding of mismatches is forbidden. Reinforces RC4.

#### CR4 — TRACEABILITY
Each discrepancy must be traceable to:
- **Ledger source** — the expected value from the authoritative ledger
- **Observed source** — the actual value from the projection or external system
Reinforces D61, D62.

#### CR5 — PROCESS COMPLETENESS
Every reconciliation must terminate in either `Completed` (all matched or all discrepancies resolved) or `Failed` (unresolvable). No reconciliation may remain in `Pending` or `InProgress` indefinitely. Reinforces D57.

### Cross-Domain D-RULES (Guard Extension)

#### D63 — RECONCILIATION DOMAIN REQUIRED
`reconciliation/process` and `reconciliation/discrepancy` must exist as fully structured DDD domains. Extends D56.

#### D64 — LEDGER AS SOURCE
Reconciliation must always reference ledger as the authoritative truth source. The `ExpectedValue` in discrepancy detection must originate from ledger data. Extends CR1.

#### D65 — DISCREPANCY TRACKING
All mismatches must be recorded as `DiscrepancyAggregate` instances. No mismatch may be silently ignored. Extends CR3, RC4.

### Cross-Domain C-CONSTRAINTS (Audit Extension)

| Code | Violation | Severity |
|---|---|---|
| **C48** | Ledger not used as authoritative source in reconciliation | S0 — CRITICAL |
| **C49** | Financial mutation detected in reconciliation context (capital/transaction/revenue) | S0 — CRITICAL |
| **C50** | Incomplete reconciliation process (stuck in Pending/InProgress) | S1 — HIGH |

---

## ENFORCEMENT & COMPLIANCE CROSS-DOMAIN RULES — 2026-04-10

These rules apply across the enforcement and compliance control layers. They govern rule definition, violation tracking, audit evidence, and the boundary between control and financial truth.

### E-RULES (Cross-Domain)

#### E1 — RULE FOUNDATION
Every violation must reference a defined enforcement rule. `ViolationAggregate.Detect()` requires a non-empty `RuleId`. `RuleAggregate` must exist and be identifiable before a violation can reference it. Violations without rule foundation are **CRITICAL violations**.

#### E2 — SOURCE TRACEABILITY
Violations and audit records must reference source domain, source aggregate, and source event where available. `ViolationAggregate.Detect()` requires a non-empty `SourceReference`. `AuditRecordAggregate.CreateRecord()` requires `SourceDomain`, `SourceAggregateId`, and `SourceEventId`. Traceability must be reconstructable from stored references.

#### E3 — NO FINANCIAL MUTATION
Enforcement and compliance cannot modify capital, ledger, transaction, revenue, or settlement truth. The enforcement context (`src/domain/economic-system/enforcement/`) and compliance context (`src/domain/economic-system/compliance/`) MUST NOT contain any reference to `LedgerEntryAggregate`, `JournalAggregate`, `VaultAggregate`, `CapitalPoolAggregate`, `TransactionAggregate`, or any write-side financial types. Control layers observe and record — they never modify.

#### E4 — TERMINAL RESOLUTION
Resolved, dismissed, and finalized states are terminal. `ViolationAggregate` in `Resolved` status accepts no further state transitions. `AuditRecordAggregate` in `Finalized` status accepts no further mutations. Any attempt to reopen or mutate a terminal record is a **CRITICAL violation**.

#### E5 — REVIEWABILITY
All violations and audit records must contain enough context for human and system review. `ViolationAggregate.Detect()` requires a non-empty `reason`. `AuditRecordAggregate.CreateRecord()` requires a non-empty `EvidenceSummary`. Records without review context are flow violations.

#### E6 — IMMUTABLE EVIDENCE
Finalized audit records are immutable. `AuditRecordAggregate.FinalizeRecord()` transitions status to `Finalized`. Any mutation attempt on a finalized record must be rejected. This is enforced by `Guard.Against(Status == AuditRecordStatus.Finalized)` in `FinalizeRecord()`.

#### E7 — UNIQUE RULE IDENTITY
`RuleCode` must be unique and stable across the system. `RuleId` constructor rejects `Guid.Empty`. Every rule must have a non-empty `Name` and `Description`. Rules are identified by their `RuleId` and must not be duplicated.

#### E8 — VIOLATION WITHOUT RULE IS FORBIDDEN
No ad hoc violation records. Every `ViolationAggregate` must reference an existing `RuleId`. `ViolationAggregate.EnsureInvariants()` enforces that `RuleId` is non-empty. Orphan violations (missing rule reference) are invariant violations.

#### E9 — AUDIT WITHOUT SOURCE IS FORBIDDEN
No orphan audit records. Every `AuditRecordAggregate` must reference a source via `SourceDomain`, `SourceAggregateId`, and `SourceEventId`. Audit records without traceable source references are structurally invalid.

#### E10 — COMPLIANCE RECORDS EVIDENCE, NOT POLICY
Compliance stores evidence of what happened, not policy evaluation logic. The compliance context contains audit records that capture facts. Policy decisions, rule evaluation, and enforcement logic belong in the enforcement context or governance layer — not in compliance.

### Enforcement & Compliance D-RULES (Guard Extension)

#### D66 — ENFORCEMENT DOMAINS REQUIRED
`economic-system/enforcement/rule` and `economic-system/enforcement/violation` must exist as fully structured DDD domains with all 7 mandatory subfolders (aggregate, entity, error, event, service, specification, value-object). Missing domain is a structural violation.

#### D67 — COMPLIANCE DOMAIN REQUIRED
`economic-system/compliance/audit` must exist as a fully structured DDD domain with all 7 mandatory subfolders. Missing domain is a structural violation.

#### D68 — RULE REFERENCE REQUIRED
Violations must reference a valid rule. `ViolationAggregate.Detect()` enforces non-empty `RuleId`. `ViolationAggregate.EnsureInvariants()` rejects empty `RuleId`. Extends E1, E8.

#### D69 — SOURCE REFERENCE REQUIRED
Violations and audit records must reference a source. `ViolationAggregate.Detect()` enforces non-empty `SourceReference`. `AuditRecordAggregate.CreateRecord()` enforces non-empty `SourceDomain`, `SourceAggregateId`, and `SourceEventId`. Extends E2, E9.

#### D70 — NO FINANCIAL MUTATION IN CONTROL LAYERS
Enforcement and compliance cannot modify financial state. `src/domain/economic-system/enforcement/` and `src/domain/economic-system/compliance/` MUST NOT reference `LedgerEntryAggregate`, `JournalAggregate`, `VaultAggregate`, `CapitalPoolAggregate`, `TransactionAggregate`. Extends E3.

#### D71 — FINALIZED IMMUTABILITY
Finalized audit records are immutable. `AuditRecordAggregate.FinalizeRecord()` rejects `Finalized` status. No further mutations after finalization. Extends E6.

#### D72 — TERMINAL STATE ENFORCEMENT
Resolved violations and finalized audit records are terminal. `ViolationAggregate.Resolve()` requires `Acknowledged` status — resolved violations accept no further transitions. `AuditRecordAggregate.FinalizeRecord()` requires `Draft` status — finalized records accept no further mutations. Extends E4.

#### D73 — RULE CODE UNIQUENESS
`RuleCode` / `RuleId` must be unique. `RuleId` constructor rejects `Guid.Empty`. No two rules may share the same identifier. Extends E7.

#### D74 — EVIDENCE COMPLETENESS
Audit records must include non-empty evidence summary and review context. `AuditRecordAggregate.CreateRecord()` requires a non-empty `EvidenceSummary`. Violations must include a non-empty `reason`. Extends E5.

### Enforcement & Compliance C-CONSTRAINTS (Violation Codes)

| Code | Violation | Severity |
|---|---|---|
| **C51** | Missing enforcement domains (`rule` or `violation` not found) | S0 — CRITICAL |
| **C52** | Missing compliance domain (`audit` not found) | S0 — CRITICAL |
| **C53** | Violation without rule reference (`RuleId` empty) | S0 — CRITICAL |
| **C54** | Violation without source reference (`SourceReference` empty) | S0 — CRITICAL |
| **C55** | Audit record without source (`SourceDomain`/`SourceAggregateId`/`SourceEventId` empty) | S0 — CRITICAL |
| **C56** | Financial mutation inside enforcement or compliance context | S0 — CRITICAL |
| **C57** | Finalized audit record mutated | S0 — CRITICAL |
| **C58** | Duplicate rule code / non-unique `RuleId` | S1 — HIGH |
| **C59** | Terminal state reopened (resolved violation or finalized audit record mutated) | S0 — CRITICAL |
| **C60** | Empty evidence summary or missing violation reason | S1 — HIGH |

### CANONICAL ENFORCEMENT & COMPLIANCE EVENT FLOW

```
Enforcement:
1. RuleDefinedEvent               → Rule created with name, description, scope (E7)
2. RuleEvaluatedEvent             → Rule evaluated against subject, pass/fail (E1)
3. ViolationDetectedEvent         → Breach recorded (E1 — RuleId, E2 — Source, E5 — Reason)
4. ViolationAcknowledgedEvent     → Violation acknowledged (E4 — requires Open status)
5. ViolationResolvedEvent         → Violation resolved (E4 — terminal, requires Acknowledged + resolution)

Compliance:
1. Source action occurs            (domain event from any economic context)
2. AuditRecordCreatedEvent        → Evidence captured (E2 — SourceDomain + SourceAggregateId + SourceEventId, E5 — EvidenceSummary)
3. AuditRecordFinalizedEvent      → Evidence sealed (E6 — immutable after this point, E4 — terminal)
```

Enforcement step 3 MUST include RuleId, SourceReference, and Reason. Step 5 is terminal. Compliance step 2 MUST include all source references and evidence summary. Step 3 is terminal.

### Enforcement & Compliance Check Procedure

1. Verify `src/domain/economic-system/enforcement/rule/` and `src/domain/economic-system/enforcement/violation/` exist with all 7 mandatory subfolders — D66.
2. Verify `src/domain/economic-system/compliance/audit/` exists with all 7 mandatory subfolders — D67.
3. Scan `ViolationAggregate.Detect()` — verify it requires non-empty `RuleId` — D68.
4. Scan `ViolationAggregate.Detect()` — verify it requires non-empty `SourceReference` — D69.
5. Scan `ViolationAggregate.Detect()` — verify it requires non-empty `reason` — D74.
6. Scan `AuditRecordAggregate.CreateRecord()` — verify it requires `SourceDomain`, `SourceAggregateId`, `SourceEventId`, and `EvidenceSummary` — D69, D74.
7. Scan `AuditRecordAggregate.FinalizeRecord()` — verify it rejects `Finalized` status — D71.
8. Grep enforcement and compliance contexts for `LedgerEntryAggregate`, `JournalAggregate`, `VaultAggregate`, `CapitalPoolAggregate`, `TransactionAggregate` — must find zero results — D70.
9. Verify `ViolationAggregate.EnsureInvariants()` rejects empty `RuleId` and `Source` — D68.
10. Verify `RuleId`, `ViolationId`, `SourceReference` constructors reject `Guid.Empty` — D68, D69.
11. Verify `ViolationAggregate.Resolve()` requires `Acknowledged` status — D72.
12. Verify `AuditRecordAggregate` accepts no mutations after `Finalized` — D71, D72.

### Enforcement & Compliance Fail Criteria

- `enforcement/rule` or `enforcement/violation` domain missing → **C51**
- `compliance/audit` domain missing → **C52**
- `ViolationAggregate` allows detection without `RuleId` → **C53**
- `ViolationAggregate` allows detection without `SourceReference` → **C54**
- `AuditRecordAggregate` allows creation without source references → **C55**
- Any file in enforcement or compliance references write-side financial types → **C56**
- `AuditRecordAggregate` allows mutation after finalization → **C57**
- `RuleId` accepts `Guid.Empty` or duplicate identifiers → **C58**
- Resolved violation or finalized audit record accepts further state changes → **C59**
- `ViolationAggregate` allows detection without reason, or `AuditRecordAggregate` allows creation without `EvidenceSummary` → **C60**

---

## CROSS-DOMAIN EXCHANGE RULES — 2026-04-10

These rules apply across all domain boundaries that interact with exchange rates and routing. They govern FX operations, rate integrity, and transaction routing.

### X-RULES (Cross-Domain)

#### X1 — RATE REQUIRED
All FX operations must reference a rate. `FxAggregate` operations that involve currency conversion MUST include a non-empty `RateId` referencing a valid `ExchangeRateAggregate`. FX without a rate reference is a **CRITICAL violation**.

#### X2 — RATE IMMUTABILITY
Active rates cannot be modified. An `ExchangeRateAggregate` in `Active` status accepts only the `Expire()` transition — no value changes, no currency changes, no version changes. Any mutation attempt on an active rate is a **CRITICAL violation**.

#### X3 — ROUTING REQUIRED
All transactions must go through routing. Transaction execution that involves cross-currency or multi-path resolution MUST pass through the routing context. Bypassing routing is a flow violation.

#### X4 — DETERMINISTIC ROUTING
Routing decisions must be reproducible. Given the same inputs (source, destination, amount, rate), routing MUST produce the same path selection. Non-deterministic routing (e.g., random path selection, system-time-based decisions) violates §9 determinism rules.

#### X5 — PATH VALIDATION
Only active paths can be selected. Routing MUST validate that selected paths are in `Active` status before execution. Selecting an inactive, suspended, or expired path is a flow violation.

### Exchange D-RULES (Guard Extension)

#### D75 — EXCHANGE RATE DOMAIN REQUIRED
`exchange/rate` must exist as a fully structured DDD domain under `economic-system` with all 7 mandatory subfolders (aggregate, entity, error, event, service, specification, value-object). Missing domain is a structural violation.

#### D76 — FX MUST USE RATE
FX operations must reference a rate. Any currency conversion operation in the `exchange/fx` context MUST include a `RateId` parameter linking to an `ExchangeRateAggregate`. FX without rate reference is structurally invalid.

#### D77 — ROUTING PATH REQUIRED
`exchange/routing` (or equivalent routing domain) must exist under `economic-system`. The routing domain provides path resolution for cross-currency and multi-hop transactions. Missing routing domain is a structural violation.

#### D78 — ROUTING MANDATORY
Transactions that cross currency boundaries or require path resolution MUST pass through routing. Direct transaction execution that bypasses routing is a flow violation.

#### D79 — RATE IMMUTABILITY
Active rates cannot be modified. `ExchangeRateAggregate` in `Active` status only accepts `Expire()`. No property mutation is permitted on active rates. This is enforced by the aggregate's state transition rules: `Active` → `Expired` only.

### Exchange C-CONSTRAINTS (Audit Extension)

| Code | Violation | Severity |
|---|---|---|
| **C61** | Missing rate domain (`exchange/rate` not found or incomplete) | S0 — CRITICAL |
| **C62** | FX operation without rate reference (`RateId` empty or missing) | S0 — CRITICAL |
| **C63** | Missing routing path domain (`exchange/routing` not found) | S1 — HIGH |
| **C64** | Transaction bypassing routing (direct execution without path resolution) | S1 — HIGH |
| **C65** | Mutable active rate (Active rate modified instead of expired and replaced) | S0 — CRITICAL |

### CANONICAL EXCHANGE EVENT FLOW

```
1. ExchangeRateDefinedEvent         → Rate created with currency pair, value, version (X1)
2. ExchangeRateActivatedEvent       → Rate becomes active (X2 — immutable after this)
3. [FX operation references rate]   → FX must include RateId (X1, D76)
4. [Routing resolves path]          → Path selected deterministically (X3, X4, X5)
5. [Transaction executes via path]  → Transaction flows through routing (X3, D78)
6. ExchangeRateExpiredEvent         → Rate retired (X2 — only valid transition from Active)
```

Step 2 makes the rate immutable (X2, D79). Step 3 MUST reference an active rate (X1, D76). Step 4 MUST select active paths only (X5). Step 5 MUST go through routing (X3, D78).

### Exchange Check Procedure

1. Verify `src/domain/economic-system/exchange/rate/` exists with all 7 mandatory subfolders — D75.
2. Verify `src/domain/economic-system/exchange/routing/` exists (or equivalent routing domain) — D77.
3. Scan `ExchangeRateAggregate` — verify Active status only permits `Expire()` — D79.
4. Scan FX context for rate references — verify FX operations require `RateId` — D76.
5. Verify routing path selection validates `Active` status — X5.
6. Verify routing produces deterministic output for identical inputs — X4.

### Exchange Fail Criteria

- `exchange/rate` domain missing or incomplete → **C61**
- FX operation in `exchange/fx` allows execution without `RateId` → **C62**
- `exchange/routing` domain missing → **C63**
- Transaction can execute without passing through routing → **C64**
- `ExchangeRateAggregate` in `Active` status accepts mutations beyond `Expire()` → **C65**
