# PROMPT: economic-system/reconciliation/discrepancy D1 Upgrade

## TITLE

D1 Upgrade — economic-system / reconciliation / discrepancy

## CONTEXT

- **Classification**: economic-system
- **Context**: reconciliation
- **Domain**: discrepancy

## OBJECTIVE

Upgrade the `discrepancy` domain to match the detailed domain spec. Add comparison data (ExpectedValue, ActualValue, Difference), source tracking (Projection/ExternalSystem), rename status lifecycle (Open/Investigating/Resolved), and replace Acknowledge with Investigate behavior.

## CONSTRAINTS

- Domain layer ZERO external dependencies ($7)
- Shared kernel: Guard, AggregateRoot, DomainEvent, Amount, Timestamp
- Namespace: `Whycespace.Domain.EconomicSystem.Reconciliation.Discrepancy`
- Events past-tense, all state changes via RaiseDomainEvent
- Reconciliation does not modify financial truth (non-mutative)

## EXECUTION STEPS

1. Replace DiscrepancyType enum with DiscrepancySource enum (Projection, ExternalSystem)
2. Rename DiscrepancyStatus values: Open, Investigating, Resolved
3. Replace DiscrepancyAcknowledgedEvent with DiscrepancyInvestigatedEvent
4. Add ExpectedValue, ActualValue, Difference (Amount) to DiscrepancyDetectedEvent
5. Update DiscrepancyAggregate: Detect() takes comparison data, Investigate() replaces Acknowledge()
6. Update errors: MissingComparisonData, InvalidDifference, InvalidStateTransition
7. Update guard rules: D60 lifecycle, add D61 comparison data, D62 source required, C46-C47

## OUTPUT FORMAT

Updated file list with changes described.

## VALIDATION CRITERIA

- DiscrepancyAggregate.Detect() requires ExpectedValue, ActualValue, DiscrepancySource, ProcessReference
- Difference computed as ExpectedValue - ActualValue
- Lifecycle: Open → Investigating → Resolved
- Investigate() only from Open status
- Resolve() from Open or Investigating
- Guard rules D60-D62, C46-C47 added to economic.guard.md
