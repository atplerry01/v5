## CLASSIFICATION: engine
## SOURCE: T1M Hardening Patch audit sweep (20260405)

### FINDING 1: Duplicate WorkflowEngine in runtime layer
- **DESCRIPTION**: `src/runtime/workflow/WorkflowEngine.cs` duplicates T1M workflow orchestration logic that should reside exclusively in `src/engines/T1M/`. Per ENGINE AUDIT CHECK-01.4, workflow orchestration belongs in T1M only.
- **PROPOSED_RULE**: Runtime layer must delegate workflow execution to T1MWorkflowEngine; no standalone workflow engine in runtime.
- **SEVERITY**: S1 (architectural)

### FINDING 2: Workflow step implementations in systems layer
- **DESCRIPTION**: `ValidateIntentStep`, `CreateTodoStep`, `EmitCompletionStep` in `src/systems/midstream/wss/steps/` implement workflow step execution logic. Per SYSTEMS AUDIT SYDIM-01.1 and ENGINE AUDIT CHECK-06.2, workflow step handlers should reside in T1M engine tier.
- **PROPOSED_RULE**: IWorkflowStep implementations must be placed in `src/engines/T1M/`, not in systems layer.
- **SEVERITY**: S1 (architectural)

### FINDING 3: Workflow event pipeline clarity
- **DESCRIPTION**: `RuntimeCommandDispatcher.ExecuteWorkflowAsync()` returns workflow events but does not explicitly invoke the persist → chain → outbox pipeline for workflow-emitted events (unlike the engine path at lines 144-146).
- **PROPOSED_RULE**: Workflow execution path in RuntimeCommandDispatcher must explicitly invoke persist → chain → outbox for accumulated events, matching the engine execution path.
- **SEVERITY**: S1 (architectural)
