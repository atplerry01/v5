# PROMPT: economic-system/risk/exposure D1 Implementation

## TITLE

D1 Implementation — economic-system / risk / exposure

## CONTEXT

- **Classification**: economic-system
- **Context**: risk
- **Domain**: exposure

## OBJECTIVE

Upgrade the `exposure` bounded context from D0 scaffold to D1 partial implementation. Implement aggregate behaviors, domain events, value objects, errors, specifications, and service per the domain specification.

## CONSTRAINTS

- Domain layer has ZERO external dependencies ($7)
- Shared kernel references only: Guard, AggregateRoot, DomainEvent, Amount, Currency, Timestamp
- Namespace: `Whycespace.Domain.EconomicSystem.Risk.Exposure`
- Events named past-tense ($10)
- All state changes emit events ($10, GE-04)
- No Guid.NewGuid() or DateTime ($9)
- Specifications are pure ($domain.guard Rule 9)
- Services are stateless ($domain.guard Rule 8)

## EXECUTION STEPS

1. Create value objects: ExposureType (enum), ExposureStatus (enum), SourceId (guarded record struct)
2. Update ExposureId with Guard validation
3. Replace D0 events with spec events: ExposureCreatedEvent, ExposureIncreasedEvent, ExposureReducedEvent, ExposureClosedEvent
4. Implement ExposureErrors with domain-specific failure messages
5. Implement ExposureAggregate extending AggregateRoot with Create, IncreaseExposure, ReduceExposure, CloseExposure
6. Implement ExposureSpecification and ExposureThresholdSpecification
7. Implement ExposureService with CalculateNetExposure

## OUTPUT FORMAT

All modified/created files listed with activation level change: D0 → D1.

## VALIDATION CRITERIA

- Aggregate extends AggregateRoot and uses RaiseDomainEvent for all state changes
- Apply method handles all four events
- EnsureInvariants enforces exposure >= 0
- Guard.Against used for all precondition checks
- No cross-BC references
- No external dependencies
- Events carry only value objects, no entity references
