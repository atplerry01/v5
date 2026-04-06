## TITLE
WBSM v3.5 — T1M Hardening Patch (Pre-Economic)

## CONTEXT
- Classification: engine
- Context: T1M (mediation)
- Domain: workflow execution pipeline

## OBJECTIVE
Make the T1M workflow engine economically safe and auditable by standardizing step results, enforcing deterministic identity and execution hashing, controlling event flow, and hardening state typing.

## CONSTRAINTS
- Steps MUST return WorkflowStepResult (with Events)
- Steps MUST NOT emit directly to AccumulatedEvents
- StepId MUST be deterministic (SHA256 of workflow:step:index)
- ExecutionHash MUST be updated per step (SHA256 chain)
- State MUST be Dictionary<string, string> (no raw object storage)
- StepType MUST be declared via WorkflowStepType enum

## EXECUTION STEPS
1. BATCH 1 — Added Events (IReadOnlyList<object>) to WorkflowStepResult
2. BATCH 2 — Added StepId to WorkflowStepDefinition
3. BATCH 3 — Created WorkflowStepType enum (Validation, Command, Transformation, Completion), added to WorkflowStepDefinition
4. BATCH 4 — Added ExecutionHash to WorkflowExecutionContext, SHA256 computation in T1MWorkflowEngine
5. BATCH 5 — Engine now collects stepResult.Events into AccumulatedEvents
6. BATCH 6 — State dictionary hardened from Dictionary<string, object?> to Dictionary<string, string>
7. Updated consumers: RuntimeCommandDispatcher, CreateTodoStep, EmitCompletionStep, ValidateIntentStep, runtime WorkflowEngine
8. Added StepType to IWorkflowStep interface

## OUTPUT FORMAT
Modified files:
- src/shared/contracts/runtime/WorkflowStepResult.cs (Events field added)
- src/shared/contracts/engine/WorkflowStepDefinition.cs (StepId, StepType added)
- src/shared/contracts/engine/WorkflowStepType.cs (NEW — enum)
- src/shared/contracts/runtime/WorkflowExecutionContext.cs (ExecutionHash, State hardened)
- src/shared/contracts/runtime/IWorkflowStep.cs (StepType added)
- src/engines/T1M/workflow-engine/WorkflowEngine.cs (SHA256 hash chain, event collection)
- src/runtime/pipeline/RuntimeCommandDispatcher.cs (StepId/StepType population)
- src/runtime/workflow/WorkflowEngine.cs (SHA256 hash chain, event collection)
- src/systems/midstream/wss/steps/todo/CreateTodoStep.cs (events via result, StepType)
- src/systems/midstream/wss/steps/todo/EmitCompletionStep.cs (StepType)
- src/systems/midstream/wss/steps/todo/ValidateIntentStep.cs (StepType)

## VALIDATION CRITERIA
- [x] Build succeeds (0 warnings, 0 errors)
- [x] Step outputs standardized (Events in WorkflowStepResult)
- [x] Events controlled (steps return events, engine collects)
- [x] Execution trace deterministic (SHA256 hash chain)
- [x] Workflow replay-safe (deterministic StepId + ExecutionHash)
- [x] State hardened (Dictionary<string, string>)
- [x] StepType declared on all steps
