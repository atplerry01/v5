# CLAUDE CODE PROMPT — EVENT FABRIC HARDENING (FINAL PASS)

## CLASSIFICATION: runtime / governance / enforcement
## MODE: SYSTEM INTEGRITY ENFORCEMENT

## TITLE
Event Fabric Hardening — Architectural Violation Fixes

## CONTEXT
Fix architectural violations in Event Fabric implementation. Rename fabric/ to event-fabric/. Split responsibilities. Add versioning, replay, guards.

## OBJECTIVE
Enforce deterministic, replayable, non-bypassable, infrastructure-decoupled runtime.

## CONSTRAINTS
- EventFabric = orchestrator ONLY
- Split into services (EventStoreService, ChainAnchorService, OutboxService)
- All events include EventName, EventVersion, SchemaHash
- Projection execution policy (INLINE/ASYNC)
- Split workers (OutboxRelay, ProjectionAsync, Replay)
- ControlPlane → Fabric is SINGLE and NON-BYPASSABLE

## EXECUTION STEPS
1. Rename fabric/ to event-fabric/
2. Split EventFabric into orchestrator + services
3. Add EventSchemaRegistry, EventVersion
4. Add ProjectionExecutionPolicy
5. Split workers
6. Strengthen ExecutionPipeline
7. Expand ExecutionHash
8. Add EventReplayService
9. Add FabricInvocationGuard
10. Update references and Program.cs

## OUTPUT FORMAT
Change report, violation fix table, compliance score

## VALIDATION CRITERIA
- Build 0 errors, 0 warnings
- No direct infra calls from EventFabric (orchestrator only)
- All events carry version metadata
- Fabric invocation non-bypassable
