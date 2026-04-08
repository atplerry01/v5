---
title: "WBSM v3.5 — TODO WSS Orchestration Enforcement Patch"
classification: orchestration
context: workflow
domain: enforcement
execution_mode: autonomous
---

## TITLE
WBSM v3.5 — TODO WSS Orchestration Enforcement Patch

## CONTEXT
Todo execution currently flows directly from Systems.Downstream (TodoIntentHandler) to Runtime (ISystemIntentDispatcher) bypassing the WSS (T1M) orchestration layer. This patch forces all Todo operations through WSS workflow orchestration.

## OBJECTIVE
Eliminate direct runtime dispatch from Systems.Downstream for Todo. All Todo operations must flow through WSS (T1M) workflow orchestration layer before reaching Runtime.

## CONSTRAINTS
- Systems.Downstream MUST NOT call RuntimeCommandDispatcher directly
- ALL Todo operations MUST go through WSS
- WSS is the ONLY entry into Runtime for Todo
- Workflow step execution MUST call Runtime (not Systems)
- No business logic inside WSS
- No domain logic inside WSS
- WSS = orchestration ONLY

## EXECUTION STEPS
1. BATCH 1: Remove direct runtime dispatch from TodoIntentHandler → use IWorkflowDispatcher
2. BATCH 2: Create IWorkflowDispatcher contract and WorkflowDispatcher implementation
3. BATCH 3: Create TodoLifecycleWorkflow definition (todo.lifecycle.create)
4. BATCH 4: Create workflow steps (ValidateIntentStep, CreateTodoStep, EmitCompletionStep)
5. BATCH 5: Create runtime workflow support (WorkflowEngine, WorkflowRegistry)
6. BATCH 6: Update RuntimeCommandDispatcher to route WorkflowStartCommand
7. BATCH 7: Verify TodoController (no changes needed)
8. BATCH 8: Add workflow policy (whyce.operational.sandbox.todo.workflow.create)

## OUTPUT FORMAT
Modified and new source files across shared/contracts, systems/midstream/wss, runtime/workflow, and systems/downstream layers.

## VALIDATION CRITERIA
- Build succeeds with 0 errors, 0 warnings
- No direct Systems → Runtime execution for Todo
- Workflow ALWAYS used for Todo operations
- WSS active in execution path
- T1M layer proven
- Runtime supports workflow commands
- Engine remains unchanged
- Domain remains pure
- No Guid.NewGuid() or DateTime.Now
- All IDs via IIdGenerator, all time via IClock

## EXECUTION TRACE
- Policy Binding: WHYCEPOLICY enforced via PolicyMiddleware (existing)
- Guard Loading: All 12 guard files loaded and validated
- Integrity Validation: PASS
- Guard Validation: PASS (structural, systems, runtime, engine, behavioral, domain, kafka, policy guards)
- Execution: COMPLETE — 14 files created, 4 files modified
- Audit Sweep: COMPLETE — findings analyzed, false positives identified, design decisions documented
- Drift Capture: No new drift rules required
- Output Validation: Build succeeded 0 errors 0 warnings

## DESIGN DECISIONS
1. IWorkflowDispatcher placed in shared/contracts/runtime/ (follows existing pattern: ICommandDispatcher, ISystemIntentDispatcher are already there)
2. WorkflowEngine placed in runtime/workflow/ (runtime routing infrastructure; EngineContext surface restriction prevents T1M engines from dispatching sub-commands through full middleware pipeline)
3. CreateTodoStep dispatches through ISystemIntentDispatcher (valid systems-layer composition via runtime)
4. CommandResult and WorkflowResult extended with Output field for workflow-to-handler data propagation without domain type references

## FILES MODIFIED
- src/shared/contracts/runtime/IWorkflowDispatcher.cs (NEW)
- src/shared/contracts/runtime/WorkflowResult.cs (NEW)
- src/shared/contracts/runtime/WorkflowStartCommand.cs (NEW)
- src/shared/contracts/runtime/IWorkflowStep.cs (NEW)
- src/shared/contracts/runtime/WorkflowStepResult.cs (NEW)
- src/shared/contracts/runtime/WorkflowExecutionContext.cs (NEW)
- src/shared/contracts/runtime/IWorkflowRegistry.cs (NEW)
- src/shared/contracts/runtime/CommandResult.cs (MODIFIED — added Output field)
- src/systems/midstream/wss/WorkflowDispatcher.cs (NEW)
- src/systems/midstream/wss/workflows/todo/TodoLifecycleWorkflow.cs (NEW)
- src/systems/midstream/wss/steps/todo/ValidateIntentStep.cs (NEW)
- src/systems/midstream/wss/steps/todo/CreateTodoStep.cs (NEW)
- src/systems/midstream/wss/steps/todo/EmitCompletionStep.cs (NEW)
- src/runtime/workflow/WorkflowEngine.cs (NEW)
- src/runtime/workflow/WorkflowRegistry.cs (NEW)
- src/runtime/pipeline/RuntimeCommandDispatcher.cs (MODIFIED — WorkflowStartCommand routing)
- src/systems/downstream/operational-system/sandbox/todo/TodoIntentHandler.cs (MODIFIED — IWorkflowDispatcher)
- src/runtime/policies/operational-system/sandbox/todo/TodoPolicyDefinition.cs (MODIFIED — WorkflowCreate policy)
