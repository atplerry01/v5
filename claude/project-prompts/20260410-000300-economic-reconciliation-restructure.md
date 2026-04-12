# PROMPT: economic-system/reconciliation Context Restructure

## TITLE

D1 Restructure — economic-system / reconciliation / {process, discrepancy}

## CONTEXT

- **Classification**: economic-system
- **Context**: reconciliation
- **Domains**: process, discrepancy

## OBJECTIVE

Replace the existing D0 `reconciliation/reconciliation` single-domain scaffold with two proper domains: `process` (orchestrates reconciliation checks) and `discrepancy` (tracks mismatches). Implement both at D1 level with full aggregate behaviors, events, value objects, errors, and specifications.

## CONSTRAINTS

- Domain layer has ZERO external dependencies ($7)
- Shared kernel references only: Guard, AggregateRoot, DomainEvent, Timestamp
- Reconciliation does not create or modify financial truth (non-mutative)
- Ledger is always authoritative
- Every reconciliation must produce a result
- Discrepancies must not be ignored
- No cross-BC references between process and discrepancy

## EXECUTION STEPS

1. Remove old `reconciliation/reconciliation/` D0 scaffold
2. Create `reconciliation/process/` domain with: ProcessAggregate (Trigger, MarkMatched, MarkMismatched, Resolve), events (Triggered, Matched, Mismatched, Resolved), value objects (ProcessId, ReconciliationStatus, SourceReference)
3. Create `reconciliation/discrepancy/` domain with: DiscrepancyAggregate (Detect, Acknowledge, Resolve), events (Detected, Acknowledged, Resolved), value objects (DiscrepancyId, DiscrepancyStatus, DiscrepancyType, ProcessReference)
4. Add guard rules and violation codes to economic.guard.md

## OUTPUT FORMAT

File tree with all created/removed artifacts listed.

## VALIDATION CRITERIA

- Old reconciliation/reconciliation removed completely
- Both domains follow three-level topology
- All 7 mandatory subfolders present in each domain
- Aggregates extend AggregateRoot, all state changes via RaiseDomainEvent
- Events use past-tense naming
- No cross-BC direct references
- No external dependencies
