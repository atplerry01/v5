# TITLE
WBSM v3.5 — Workflow State Eventification (Runtime → Domain + Projection)

# CONTEXT
Classification: orchestration-system / workflow / lifecycle
Layer impact: domain (NEW), engines/T1M (MODIFIED), runtime (CLEANUP), projections (NEW), shared/contracts (NEW + DELETED), platform/host (REWIRED), tests (REWIRED), guards/audits (UPDATED)
Supersedes: 20260406-000000-runtime-workflow-state-persistence.md

# OBJECTIVE
Eliminate runtime-owned workflow state. Restore canonical WBSM layering:
runtime = execution only, domain = lifecycle truth, projections = read model,
events = the only mechanism for state transition.

# CONSTRAINTS
- Runtime layer purity ($7): runtime must NOT own state, persist, or mutate domain
- Engine guard rule 3: T1M must NOT call mutating methods on aggregates
- Runtime guard 11.R-DOM-01: runtime must NOT reference Whycespace.Domain.* concrete types
- Determinism: no Guid.NewGuid / DateTime.UtcNow in domain or engine code
- No bypass of WHYCEPOLICY ($8) — workflow lifecycle events flow through the runtime middleware pipeline

# EXECUTION STEPS
1. Marked the prior runtime-side prompt SUPERSEDED.
2. Created the workflow execution domain under `src/domain/orchestration-system/workflow/execution/`:
   - `aggregate/WorkflowExecutionAggregate.cs` (replay target with Start/CompleteStep/Complete/Fail + Apply)
   - `value-object/WorkflowExecutionStatus.cs`, `value-object/WorkflowExecutionId.cs`
   - `event/WorkflowExecutionStartedEvent.cs`, `WorkflowStepCompletedEvent.cs`, `WorkflowExecutionCompletedEvent.cs`, `WorkflowExecutionFailedEvent.cs`
   - `error/WorkflowExecutionErrors.cs`
3. Added shared contracts:
   - `src/shared/contracts/runtime/IDomainEventSink.cs` (canonical replacement for IWorkflowStepObserver)
   - `src/shared/contracts/projections/orchestration-system/workflow/IWorkflowExecutionProjectionStore.cs` + `WorkflowExecutionReadModel.cs`
   - `src/shared/contracts/events/orchestration-system/workflow/WorkflowExecutionEventSchemas.cs` (4 schema records for projection consumption)
4. Created `src/engines/T1M/lifecycle/WorkflowLifecycleEventFactory.cs` — produces lifecycle events without calling aggregate mutators (engine.guard rule 3 compliance).
5. Updated `src/shared/contracts/runtime/WorkflowExecutionContext.cs`:
   - Removed `StepObserver` property
   - Implemented `IDomainEventSink` (`EmitEvent`, `EmitEvents`, `EmittedEvents`)
6. Rewrote `src/engines/T1M/workflow-engine/WorkflowEngine.cs`:
   - Injects `WorkflowLifecycleEventFactory`
   - Emits Started → StepCompleted* → (Completed | Failed) into the context sink
   - No observer calls remain
7. Rewrote `src/runtime/dispatcher/RuntimeCommandDispatcher.cs`:
   - Removed `IWorkflowStateRepository` and `IClock` dependencies
   - Workflow execution path returns accumulated events for runtime persist→chain→outbox
   - `WorkflowResumeCommand` now returns a structured failure pending `IWorkflowExecutionReplayService` (R-WF-RESUME-01)
8. Created the projection layer:
   - `src/projections/orchestration-system/workflow/handler/WorkflowExecutionProjectionHandler.cs` (envelope + 4 typed handlers)
   - `src/platform/host/adapters/InMemoryWorkflowExecutionProjectionStore.cs` (PLACEHOLDER per T-PLACEHOLDER-01)
   - `scripts/migrations/002_create_workflow_execution_projection.sql` (matching migration)
9. Created `src/platform/host/composition/orchestration/workflow/WorkflowExecutionBootstrap.cs`:
   - Registers projection store + handler + 4 schema/payload-mapper pairs
10. Registered the new bootstrap in `src/platform/host/composition/BootstrapModuleCatalog.cs`.
11. Updated `RuntimeComposition.cs` to register `WorkflowLifecycleEventFactory`.
12. Updated `InfrastructureComposition.cs` to drop `IWorkflowStateRepository` registration.
13. Updated `tests/integration/setup/TestHost.cs` and `NoOpWorkflowStubs.cs` to drop the workflow state repository.
14. Deleted:
    - `src/runtime/workflow-state/` (5 files) and the directory itself
    - `src/shared/contracts/runtime/IWorkflowStateRepository.cs`
    - `src/shared/contracts/runtime/IWorkflowStepObserver.cs`
15. Updated guards:
    - `runtime.guard.md`: revoked R-WF-OBSERVER-01, added R-WF-EVENTIFIED-01 + R-WF-RESUME-01
    - `engine.guard.md`: added E-LIFECYCLE-FACTORY-01
    - `projection.guard.md`: added PROJ-WF-EXEC-01
16. Updated audits:
    - `runtime.audit.md`: added CHECK R-WF-EVENTIFIED-01 + R-WF-RESUME-01
    - `engine.audit.md`: added CHECK E-LIFECYCLE-FACTORY-01

# OUTPUT FORMAT
- 16 new files
- 9 modified files
- 7 deleted files
- `dotnet build` — full solution: 0 errors, 0 new warnings

# VALIDATION CRITERIA
- Build: full solution green (verified at end of execution)
- `src/runtime/workflow-state/` does not exist
- `IWorkflowStateRepository` / `IWorkflowStepObserver` do not exist
- `RuntimeCommandDispatcher` constructor does not name `IWorkflowStateRepository`
- `WorkflowExecutionContext` has no `StepObserver` property
- Lifecycle events live in `src/domain/orchestration-system/workflow/execution/event/`
- `WorkflowLifecycleEventFactory` lives in `src/engines/T1M/lifecycle/`
- Projection handler lives in `src/projections/orchestration-system/workflow/`
- Audit sweep is run after this prompt completes; new drift captured under `claude/new-rules/`
