# TITLE
WBSM v3.5 — Final T1M/Runtime Cleanup (S1 Fix)

# CONTEXT
Classification: engine / runtime / structural
Context: T1M workflow engine, Runtime command pipeline, Systems step relocation
Domain: Operational System (Todo vertical slice)

# OBJECTIVE
Eliminate duplicate workflow engines, fix step placement in correct layer, lock event ownership to runtime, and wire all DI registrations for workflow execution path.

# CONSTRAINTS
- T1M is the ONLY workflow execution engine
- Runtime MUST NOT contain workflow engine logic
- Steps MUST live in engines/T1M
- Runtime is the ONLY event persistence layer
- No architecture changes beyond scope (§5 anti-drift)
- Dependency direction: platform > systems > runtime > engines > domain < shared

# EXECUTION STEPS
1. Guard Loading — loaded 14 guard files from /claude/guards/
2. BATCH 1 — Deleted duplicate WorkflowEngine from src/runtime/workflow/ and legacy TodoWorkflow.cs
3. BATCH 2 — Moved 3 workflow steps from systems/midstream/wss/steps/todo/ to engines/T1M/steps/todo/
4. BATCH 2 — Moved CreateTodoIntent DTO from systems to shared/contracts (dependency direction fix)
5. BATCH 2 — Simplified TodoLifecycleWorkflow to name constants only (systems = composition only)
6. BATCH 2 — Added DI registrations in Program.cs: T1MWorkflowEngine, WorkflowStepExecutor, steps, IWorkflowRegistry, IWorkflowDispatcher
7. BATCH 3 — Verified event pipeline lock: zero persistence/outbox/chain references in engines layer
8. BATCH 4 — Build validated: 0 errors, 0 warnings. All structural assertions pass.
9. Audit Sweep — 14 pre-existing violations captured. Zero new violations introduced.
10. New Rules — 3 S1 findings captured in /claude/new-rules/20260405-230000-guards.md

# OUTPUT FORMAT
Structured execution report with file-level change manifest.

# VALIDATION CRITERIA
✓ Only one WorkflowEngine exists (T1MWorkflowEngine in engines/T1M)
✓ Steps only exist in engines/T1M/steps/todo/
✓ Systems contain no execution logic (composition only)
✓ Runtime owns event pipeline (EventStore → ChainAnchor → Outbox)
✓ No bypass paths
✓ Build succeeds with 0 errors, 0 warnings
✓ No new audit violations introduced
