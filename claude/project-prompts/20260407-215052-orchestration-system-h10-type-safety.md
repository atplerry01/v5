# WBSM v3.5 — H10 TYPE SAFETY + TEST HARNESS PATCH (CLAUDE-READY)

# CLASSIFICATION: orchestration-system / workflow / integrity
# TYPE: TYPE REGISTRY + EVENT ENRICHMENT + DESERIALIZATION + TEST HARNESS
# MODE: STRICT / DETERMINISTIC / NO-DRIFT / BACKWARD-COMPATIBLE

## OBJECTIVE (LOCKED)

Finalize workflow system hardening by:
1. Making Payload and StepOutputs type-safe across storage boundaries
2. Ensuring Postgres replay = In-memory replay
3. Introducing deterministic T1M test harness
4. Preserving backward compatibility (additive only)

## H10.1 — TYPE REGISTRY (CORE)
- CREATE src/shared/kernel/serialization/IPayloadTypeRegistry.cs (GetName / Resolve)
- CREATE src/platform/host/adapters/PayloadTypeRegistry.cs (Dictionary<string,Type> map; Register<T>)
- RULE: All payload/output types MUST be registered

## H10.2 — EVENT ENRICHMENT (ADDITIVE)
- MODIFY WorkflowExecutionStartedEvent: add string? PayloadType
- MODIFY WorkflowStepCompletedEvent: add string? OutputType

## H10.3 — FACTORY TYPE INJECTION
- MODIFY WorkflowLifecycleEventFactory: inject IPayloadTypeRegistry
- Started/StepCompleted populate PayloadType/OutputType via _registry.GetName

## H10.4 — EVENT DESERIALIZATION FIX
- MODIFY src/platform/host/adapters/PostgresEventStoreAdapter.cs
- Inject IPayloadTypeRegistry; on read, if Payload/Output is JsonElement and *Type set, deserialize via Resolve

## H10.5 — DI REGISTRATION
- MODIFY RuntimeComposition.cs: AddSingleton<IPayloadTypeRegistry, PayloadTypeRegistry>

## H10.6 — T1M TEST HARNESS
- CREATE tests/t1m/workflow/TestWorkflowSteps.cs (StepA, StepB)
- CREATE tests/t1m/workflow/WorkflowTestHarness.cs (T1MWorkflowEngine + InMemoryEventStore + registry wiring)

## H10.7 — TESTS
- Resume midway -> "AB"
- Payload integrity round-trip
- Output integrity "AB"
- Determinism: run1Outputs == run2Outputs

## GUARDS (engine.guard.md)
- E-TYPE-01 Payload/Output types MUST be registered
- E-TYPE-02 Events MUST include PayloadType/OutputType
- E-TYPE-03 EventStore MUST restore typed objects from JSON

## AUDITS (engine.audit.md)
- CHECK-E-TYPE-01 PayloadType missing when Payload exists
- CHECK-E-TYPE-02 JsonElement returned without deserialization
- CHECK-E-TYPE-03 Unknown type not handled

## RESULT
Type-safe, replay-consistent, testable, production-complete workflow engine.
