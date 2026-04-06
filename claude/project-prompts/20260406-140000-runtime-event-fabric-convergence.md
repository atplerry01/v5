# CLAUDE CODE PROMPT — RUNTIME REFACTOR TO EVENT FABRIC (WBSM v3.5)

## CLASSIFICATION: runtime / governance / enforcement
## MODE: SYSTEM REWRITE (CONTROLLED, GUARDED)

## TITLE
Runtime Event Fabric Convergence — Transform to Deterministic Execution Fabric

## CONTEXT
Transform runtime from messaging-based execution to Event Fabric-based deterministic execution system.

## OBJECTIVE
Replace direct EventStore/ChainAnchor/Outbox calls with Event Fabric pipeline. Remove Kafka as runtime concern. Add first-class projection dispatch, deterministic hashing core, and execution pipeline.

## CONSTRAINTS
- Zero drift, full alignment, no assumption
- Kafka becomes infrastructure adapter ONLY
- Event Fabric handles all post-execution processing
- Projections dispatched by fabric, NOT by Kafka
- All events include deterministic EventId, ExecutionHash, PolicyHash

## EXECUTION STEPS
1. Delete messaging layer (KafkaPublisher, DeterministicPartitionResolver)
2. Create Event Fabric (7 files)
3. Create Projection layer (5 files)
4. Create Deterministic core (2 files)
5. Create ExecutionPipeline
6. Add guards (MiddlewareOrderGuard, ControlPlaneGuard)
7. Update RuntimeControlPlane to delegate to EventFabric
8. Update Program.cs to wire EventFabric
9. Remove old persistence adapters (superseded by fabric)
10. Build verification

## OUTPUT FORMAT
Change report, structure tree, violation fixes, compliance score

## VALIDATION CRITERIA
- Build succeeds with 0 errors, 0 warnings
- No Kafka references in runtime
- No direct EventStore/Chain/Outbox calls from ControlPlane
- Determinism enforcement clean
