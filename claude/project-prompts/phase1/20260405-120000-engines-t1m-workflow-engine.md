# TITLE
T1M Enterprise Workflow Engine Activation

# CONTEXT
Classification: engines / T1M / orchestration
The runtime-level WorkflowEngine was tightly coupled to the runtime layer. T1M moves workflow orchestration into the engines tier as a proper T1M (Tier 1 Midstream) engine, separating orchestration from persistence/routing concerns.

# OBJECTIVE
Implement the T1M enterprise-grade workflow execution engine that replaces the runtime-level WorkflowEngine with a proper engines-tier orchestrator.

# CONSTRAINTS
1. T1M MUST NOT contain business logic
2. T1M MUST NOT persist domain data
3. T1M executes workflows, not define them
4. T1M MUST support step isolation
5. T1M MUST support recursive runtime invocation
6. T1M MUST be deterministic

# EXECUTION STEPS
1. Created shared contracts: IWorkflowEngine, WorkflowDefinition, WorkflowStepDefinition, WorkflowExecutionResult
2. Enhanced WorkflowExecutionContext with WorkflowId, IdentityId, PolicyDecision, CurrentStepIndex, State
3. Created T1MWorkflowEngine in engines/T1M/workflow-engine/
4. Created WorkflowStepExecutor in engines/T1M/step-executor/
5. Updated RuntimeCommandDispatcher to use IWorkflowEngine (T1M) instead of runtime WorkflowEngine
6. Added Microsoft.Extensions.DependencyInjection.Abstractions to engines project

# OUTPUT FORMAT
Working T1M engine implementation with compilation verified.

# VALIDATION CRITERIA
- T1M exists in engines layer at src/engines/T1M/
- Workflow execution routed from RuntimeCommandDispatcher to T1M
- Step handlers isolated via WorkflowStepExecutor
- Workflow context maintained with enhanced fields
- Recursive runtime calls supported via existing ISystemIntentDispatcher pattern
- No business logic in T1M
- Build succeeds with zero errors
