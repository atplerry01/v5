# workflow.guard.md

## NEW RULES INTEGRATED — 2026-04-07 (NORMALIZATION)

### RULE: WF-PLACEMENT-01 — WORKFLOW EXECUTION LAYER LOCK
Workflow execution MUST occur ONLY in T1M engines.

ENFORCEMENT:
- systems layer MUST be declarative only
- runtime MUST NOT implement workflow logic
- engines/T1M is the ONLY execution layer

---

### RULE: WF-TYPE-01 — WORKFLOW CLASSIFICATION
Only TWO workflow types are allowed:

1. Operational Workflow (short-term execution)
2. Lifecycle Workflow (state transition orchestration)

ENFORCEMENT:
- All workflows must be classified explicitly
- No hybrid undefined workflows

---

### RULE: WF-PIPELINE-01 — WORKFLOW MUST PASS FULL PIPELINE
All workflow execution MUST pass through runtime control plane.

ENFORCEMENT:
- WorkflowStartCommand MUST go through: Guard → Policy → Idempotency → Execution
