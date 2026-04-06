# TITLE
WBSM v3.5 — Workflow State Persistence (Runtime Control Plane)

# CONTEXT
Classification: runtime / workflow-state / persistence
Layer: Runtime
Dependencies: IClock, IIdGenerator, IWorkflowStateRepository, WorkflowExecutionContext

# OBJECTIVE
Persist workflow execution state for replay, recovery, and long-running workflows. Runtime owns all persistence; T1M remains stateless.

# CONSTRAINTS
- T1M MUST NOT persist state
- Runtime MUST persist workflow state
- Workflow definitions MUST NOT be stored
- State MUST be deterministic and serializable
- WorkflowId MUST be deterministic
- No Guid.NewGuid, no system time (use IClock)
- All operations through WHYCEPOLICY

# EXECUTION STEPS
1. Created WorkflowState model + WorkflowStatus enum in src/runtime/workflow-state/
2. Created IWorkflowStateRepository contract + WorkflowStateRecord in src/shared/contracts/runtime/
3. Created WorkflowStateRepository (in-memory ConcurrentDictionary) in src/runtime/workflow-state/
4. Created IWorkflowStepObserver contract for step lifecycle callbacks
5. Created WorkflowStateObserver (runtime persistence implementation)
6. Created WorkflowStateSerializer for deterministic JSON serialization
7. Updated WorkflowExecutionContext with optional StepObserver property
8. Updated T1MWorkflowEngine to invoke observer on step complete/workflow complete/workflow fail
9. Updated T1MWorkflowEngine to support resume from CurrentStepIndex
10. Updated RuntimeCommandDispatcher to create initial state, wire observer, and handle resume
11. Created WorkflowResumeCommand record
12. Created database migration SQL for workflow_state table

# OUTPUT FORMAT
New files created, existing files modified. Build succeeds with 0 warnings, 0 errors.

# VALIDATION CRITERIA
- Workflow state persisted after each step
- Resume capability works via WorkflowResumeCommand
- T1M remains stateless (observer pattern, no direct persistence)
- Runtime owns persistence
- Deterministic serialization (sorted dictionary keys)
- Deterministic replay possible via ExecutionHash chain
