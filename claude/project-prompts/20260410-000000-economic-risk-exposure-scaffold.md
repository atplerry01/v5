# PROMPT: economic-system/risk/exposure D0 Scaffold

## TITLE

D0 Scaffold — economic-system / risk / exposure

## CONTEXT

- **Classification**: economic-system
- **Context**: risk
- **Domain**: exposure

## OBJECTIVE

Create a D0 scaffold for the `exposure` bounded context under the `risk` context within `economic-system`. The exposure domain tracks financial risk and capital commitments, providing system-wide visibility into financial exposure.

## CONSTRAINTS

- Domain layer has ZERO external dependencies ($7)
- All artifacts must follow canonical DDD structure (domain.guard.md)
- Namespace: `Whycespace.Domain.EconomicSystem.Risk.Exposure`
- Three-level topology: `src/domain/economic-system/risk/exposure/`
- Mandatory subfolders: aggregate, entity, error, event, service, specification, value-object
- D0 activation level — scaffold with minimal implementations
- Deterministic IDs only — no Guid.NewGuid() ($9)
- Events named past-tense ($10)

## EXECUTION STEPS

1. Create folder structure: `src/domain/economic-system/risk/exposure/{aggregate,entity,error,event,service,specification,value-object}/`
2. Create `ExposureAggregate.cs` with Create factory, EnsureInvariants, ValidateBeforeChange
3. Create events: `ExposureCreatedEvent`, `ExposureUpdatedEvent`, `ExposureStateChangedEvent`
4. Create `ExposureId` value object (readonly record struct)
5. Create `ExposureErrors` static class
6. Create `ExposureService` sealed class
7. Create `ExposureSpecification` sealed class
8. Create `entity/.gitkeep` placeholder

## OUTPUT FORMAT

File tree with all created artifacts listed.

## VALIDATION CRITERIA

- Three-level topology enforced
- All seven mandatory subfolders present
- Namespace matches folder path
- Events use past-tense naming
- Value objects are immutable (record struct)
- No external dependencies
- No Guid.NewGuid() or DateTime references
